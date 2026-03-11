using CrmCorner.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace CrmCorner.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.Username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendEmailCalendarAsync(MailMessage mailMessage)
        {
            var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendEmailConfirmationAsync(string to, string confirmationLink)
        {
            var subject = "Email Doğrulama";

            var body = $@"
        <h3>Email adresinizi doğrulayın</h3>
        <p>Hesabınızı doğrulamak için aşağıdaki linke tıklayın:</p>
        <p><a href='{confirmationLink}'>Email adresimi doğrula</a></p>";

            await SendEmailAsync(to, subject, body);
        }

    }
}
