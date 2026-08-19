using System.Net;
using System.Net.Mail;

namespace WebApi.Services
{
    public class EmailService
    {
        private readonly IConfiguration configuration;
        private readonly SmtpClient smtpClient;
        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
            smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(configuration["Mail"], configuration["MailPassword"])

            };

        }
        public async Task SendMail(string message, string to)
        {
            MailMessage msg = new MailMessage
            {
                From = new MailAddress(configuration["Mail"]!),
                Subject = "Email verification",
                Body = message
            };

            msg.To.Add(to);
            await smtpClient.SendMailAsync(msg);
        }
    }
}