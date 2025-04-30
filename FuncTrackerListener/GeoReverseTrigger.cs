using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using trackerlistener.DataAccessLayer;
using trackerlistener.Services;
using AITrackerAgent.Interfaces;

namespace trackerlistener.function
{
    public class GeoReverseTrigger
    {
        private readonly ILogger _logger;

        private readonly ApplicationDbContext dbContext;
        private readonly DBTrackerService dbTrackerService;

        public GeoReverseTrigger(ILoggerFactory loggerFactory, IAddressService addressService)
        {
            _logger = loggerFactory.CreateLogger<GeoReverseTrigger>();
            var dbConnection = Environment.GetEnvironmentVariable("SqlConnectionString");
            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(dbConnection)
                .Options;

            dbContext = new ApplicationDbContext(contextOptions);
            dbTrackerService = new DBTrackerService(dbContext, addressService);
        }



        [Function("GeoReverseTrigger")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# GeoReverseTrigger trigger function executed at: {DateTime.Now}");
            await dbTrackerService.BatchUpdateAddress();

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next GeoReverseTrigger timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
