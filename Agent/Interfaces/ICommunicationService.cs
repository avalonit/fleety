using SendGrid;

namespace AITrackerAgent.Interfaces;

public interface ICommunicationService
{
    Task<Response> SendMail(string emailTo, string driverName, string subject, string htmlBody, string plainBody);
}

