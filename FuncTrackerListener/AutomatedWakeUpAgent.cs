using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using trackerlistener.DataAccessLayer;
using trackerlistener.Services;
using AITrackerAgent.Interfaces;

namespace trackerlistener.function
{
    public class AutomatedWakeUpAgent
    {
        private readonly ILogger _logger;
        private readonly DBTrackerService dbTrackerService;
        private readonly ApplicationDbContext dbContext;
        private readonly IAgentService agentChatbotService;
        private readonly ICommunicationService communicationService;

        public AutomatedWakeUpAgent(ILoggerFactory loggerFactory, IAgentService agentChatbotService, IAddressService addressService, ICommunicationService communicationService)
        {
            _logger = loggerFactory.CreateLogger<AutomatedWakeUpAgent>();
            var dbConnection = Environment.GetEnvironmentVariable("SqlConnectionString");
            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(dbConnection)
                .Options;

            dbContext = new ApplicationDbContext(contextOptions);
            dbTrackerService = new DBTrackerService(dbContext, addressService);
            this.agentChatbotService = agentChatbotService;
            this.communicationService = communicationService;
        }

        [Function("AutomatedWakeUpAgent")]
        public async Task Run([TimerTrigger("0 0 6 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# AutomatedWakeUpAgent trigger function executed at: {DateTime.Now}");
            var agentHelper = new AgentHelper(dbContext, dbTrackerService, agentChatbotService, communicationService);
            await agentHelper.Run();
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next AutomatedWakeUpAgent schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }

    }
}
