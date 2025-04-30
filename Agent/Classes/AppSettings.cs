

namespace AITrackerAgent.Classes;

public class AppSettings
{
    public string? AzureOpenAIEndpointEmbedding { get; set; }
    public string? AzureOpenAIEndpointChat { get; set; }
    public string? AzureOpenAIApiKeyEmbedding { get; set; }
    public string? AzureOpenAIApiKeyChat { get; set; }
    public string? EmbeddingModelDeploymentName { get; set; }
    public string? ChatModelDeploymentName { get; set; }
    public string? SqlConnectionString { get; set; }
    public string? SqlChatMemoryTableName { get; set; }
    public string? AtlasMapKey { get; set; }
    public string? SyncfusionLicense { get; set; }
    public string? SendGridKey { get; set; }
    public string? SendGridSender { get; set; }
    



}
