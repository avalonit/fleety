using System.Text;
using AITrackerAgent.Classes;
using AITrackerAgent.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Microsoft.SemanticKernel.Memory;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

namespace AITrackerAgent.Services;

public class AgentService : IAgentService
{
    private readonly string embeddingModelDeploymentName;
    private readonly string chatModelDeploymentName;
    private readonly string sqlConnectionString;
    private readonly string sqlChatMemoryTableName;

    private readonly string azureOpenAIEndpointChat;
    private readonly string azureOpenAIApiKeyChat;
    private readonly string azureOpenAIEndpointEmbedding;
    private readonly string azureOpenAIApiKeyEmbedding;

    private ChatHistory chat;
    private ISemanticTextMemory memory;
    private List<string> localMemory;
    private IChatCompletionService chatCompletionService;
    private AzureOpenAIPromptExecutionSettings openAIPromptExecutionSettings;
    private Kernel kernel;
    private IAddressService addressService;
    private ICommunicationService communicationService;

    public AgentService(IAddressService addressService, ICommunicationService communicationService, IConfiguration configuration)
    {
        var settings = configuration.Get<AppSettings>();

#pragma warning disable CS8601, CS8602  // Possible null reference assignment.
        azureOpenAIEndpointEmbedding = settings.AzureOpenAIEndpointEmbedding;
        azureOpenAIEndpointChat = settings.AzureOpenAIEndpointChat;
        azureOpenAIApiKeyEmbedding = settings.AzureOpenAIApiKeyEmbedding;
        azureOpenAIApiKeyChat = settings.AzureOpenAIApiKeyChat;
        embeddingModelDeploymentName = settings.EmbeddingModelDeploymentName;
        chatModelDeploymentName = settings.ChatModelDeploymentName;
        sqlConnectionString = settings.SqlConnectionString;
        sqlChatMemoryTableName = settings.SqlChatMemoryTableName;
#pragma warning restore CS8601, CS8602 // Possible null reference assignment.
        this.addressService = addressService;
        this.communicationService = communicationService;
    }

    public async Task CreateChatBot()
    {
        openAIPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var handler = new HttpClientHandler();
        handler.CheckCertificateRevocationList = false;
        var httpClient = new HttpClient(handler);

        var sc = new ServiceCollection();
        sc.AddAzureOpenAIChatCompletion(chatModelDeploymentName, azureOpenAIEndpointChat, azureOpenAIApiKeyChat, null, null, null, httpClient);
        sc.AddKernel();
        var services = sc.BuildServiceProvider();
        memory = new MemoryBuilder()
                .WithSqlServerMemoryStore(sqlConnectionString)
                .WithTextEmbeddingGeneration(
                    (loggerFactory, httpClient) =>
                    {
                        return new AzureOpenAITextEmbeddingGenerationService(
                                embeddingModelDeploymentName,
                                azureOpenAIEndpointEmbedding,
                                azureOpenAIApiKeyEmbedding,
                                modelId: null,
                                httpClient: httpClient,
                                loggerFactory: loggerFactory,
                                dimensions: 1536
                        );
                    }
                )
                .Build();
        localMemory = new List<string>();

        kernel = services.GetRequiredService<Kernel>();
        kernel.Plugins.AddFromObject(new AgentSessionPlugin(kernel, memory, addressService, communicationService, sqlConnectionString));
        chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        chat = new ChatHistory($"You are an AI assistant that helps to find a vehicle position and to find information about its driver. " +
            $"Use a professional tone when aswering and provide a summary of data instead of lists. If users ask about topics you don't know, " +
            $"answer that you don't know. Today's date is {DateTime.Now:yyyy-MM-dd}. " +
            $"Query the database at every user request, even if information is available in chat history, " +
            $"to make sure you always have the latest information." +
            $"If the user make question about showing on a map, and only in this case, reply with a JSON with those two field: BingUrl and Address.");

    }

    public async Task AddMemory(string question, string questionId)
    {
        await memory.SaveInformationAsync(sqlChatMemoryTableName, question, questionId);
    }

    public void AddLocalMemory(string question)
    {
        localMemory.Add(question);
    }

    public async Task<string> AddMessage(string question)
    {
        var builder = new StringBuilder();

        builder.Clear();
        await foreach (var result in memory.SearchAsync(sqlChatMemoryTableName, question, limit: 3, minRelevanceScore: 0.35))
        {
            builder.AppendLine(result.Metadata.Text);
        }
        foreach (var result in localMemory)
        {
            builder.AppendLine(result);
        }
        if (builder.Length > 0)
        {
            builder.Insert(0, "Here's some additional information you can use to answer the question: ");
            chat.AddSystemMessage(builder.ToString());
        }

        builder.Clear();
        chat.AddUserMessage(question);
        await foreach (var message in chatCompletionService.GetStreamingChatMessageContentsAsync(chat, openAIPromptExecutionSettings, kernel))
        {
            builder.Append(message.Content);
        }

        chat.AddAssistantMessage(builder.ToString());

        return builder.ToString();
    }
}

