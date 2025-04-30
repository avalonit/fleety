using AITrackerAgent.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;
using SendGrid;
using AITrackerAgent.Classes;

namespace AITrackerAgent.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IConfiguration configuration;

        public CommunicationService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<Response> SendMail(string emailTo, string driverName, string subject, string htmlBody, string plainBody)
        {
            var settings = configuration.Get<AppSettings>();
            var client = new SendGridClient(settings.SendGridKey);
            var from = new EmailAddress(settings.SendGridSender, "Tracker");
            var to = new EmailAddress(emailTo, driverName);
            var plainTextContent = plainBody;
            var htmlContent = htmlBody;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            return await client.SendEmailAsync(msg);
        }

    }

  
}
