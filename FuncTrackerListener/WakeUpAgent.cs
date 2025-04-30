using AITrackerAgent.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using trackerlistener.DataAccessLayer;
using trackerlistener.Services;

namespace trackerlistener.functions
{
    public class WakeUpAgent
    {
        private readonly ILogger<TrackerTrigger> _logger;
        private readonly DBTrackerService dbTrackerService;
        private readonly ApplicationDbContext dbContext;
        private readonly IAgentService agentChatbotService;
        private readonly ICommunicationService communicationService;

        public WakeUpAgent(ILogger<TrackerTrigger> logger, IAgentService agentChatbotService, IAddressService addressService, ICommunicationService communicationService)
        {
            _logger = logger;
            var dbConnection = Environment.GetEnvironmentVariable("SqlConnectionString");
            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(dbConnection)
                .Options;

            dbContext = new ApplicationDbContext(contextOptions);
            dbTrackerService = new DBTrackerService(dbContext, addressService);
            this.agentChatbotService = agentChatbotService;
            this.communicationService = communicationService;
        }

        [Function("WakeUpAgent")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            var agentHelper = new AgentHelper(dbContext, dbTrackerService, agentChatbotService, communicationService);
            await agentHelper.Run();

            return new OkResult();
        }
    }
}
