using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Listener.Models;
using Microsoft.EntityFrameworkCore;
using trackerlistener.DataAccessLayer;
using trackerlistener.Services;
using AITrackerAgent.Interfaces;

namespace trackerlistener.functions
{
    public class TrackerTrigger
    {
        private readonly ILogger<TrackerTrigger> _logger;
        private readonly ApplicationDbContext dbContext;
        private readonly DBTrackerService dbTrackerService;

        public TrackerTrigger(ILogger<TrackerTrigger> logger, IAddressService addressService)
        {
            _logger = logger;

            var dbConnection = Environment.GetEnvironmentVariable("SqlConnectionString");
            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(dbConnection)
                .Options;

            dbContext = new ApplicationDbContext(contextOptions);
            dbTrackerService = new DBTrackerService(dbContext, addressService);
        }

        [Function("TrackerTrigger")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            var requestBody = string.Empty;
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation($"Received payload: {requestBody}");

                var trackerEvent = JsonSerializer.Deserialize<TrackerEvent>(
                    requestBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (trackerEvent == null)
                {
                    _logger.LogWarning("Deserialization resulted in a null object");
                    return new BadRequestObjectResult("Invalid event data format");
                }

                _logger.LogInformation($"Successfully deserialized tracker event with ID: {trackerEvent.id}");
                if (trackerEvent.name != null)
                {
                    var isEventAdded = dbTrackerService.Update(trackerEvent);
                    _logger.LogInformation($"Event {trackerEvent.name} {trackerEvent.equipmentid} added = {isEventAdded}");
                }

            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing the request body");
                return new BadRequestObjectResult("Invalid JSON format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the request");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new OkObjectResult(requestBody);
        }
    }
}
