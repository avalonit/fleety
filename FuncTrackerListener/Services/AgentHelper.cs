using AITrackerAgent.Classes;
using AITrackerAgent.Interfaces;
using System.Text.Json;
using trackerlistener.DataAccessLayer;

namespace trackerlistener.Services;

public class AgentHelper(ApplicationDbContext dbContext, DBTrackerService dbTrackerService, IAgentService agentChatbotService, ICommunicationService communicationService)
{
    public async Task Run()
    {
        await agentChatbotService.CreateChatBot();
        var drivers = await dbTrackerService.GetDriversAsync();
        foreach(var driver in drivers)
        {
            var messageInfo = $"Tell me the current address, the last update time, and the speed of the driver with name {driver.driver_name}";
            var resultDriverLog = await agentChatbotService.AddMessage(messageInfo);
            var messageGoogleMap = $"Give me the google map location of the driver with name {driver.driver_name}";

            var messageGoogleMapText= string.Empty;
            var messageGoogleMapLink = string.Empty;
            var resultMaps = await agentChatbotService.AddMessage(messageGoogleMap);
            var bingMapJson = JsonSerializer.Deserialize<BingData>(resultMaps);
            if (bingMapJson != null)
            {
                messageGoogleMapText = bingMapJson.Address;
                messageGoogleMapLink = bingMapJson.BingUrl;
            }
            else
                messageGoogleMapText = resultMaps;

            var emailTo = driver.driver_email;
            var subject = "Good morning";
            var driverName = driver.driver_name;
            var htmlBody = $"<p>Good morning {driverName},</p><p>{resultDriverLog}</p><br>Current Location <a href=\"\"{messageGoogleMapLink}\"\"></a>{messageGoogleMapText}<p>Have a nice day!</p>";
            var plainBody = $"Good morning {driverName},\n{resultDriverLog}\nCurrent Location{resultMaps}\nHave a nice day!";
            await communicationService.SendMail(emailTo, driverName, subject, htmlBody, plainBody);
        }
    }

}