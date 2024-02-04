using CrmCorner.OptionsModels;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CrmCorner.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly EmailSettings _emailSetings;

        public EmailServices(IOptions<EmailSettings> options)
        {
            _emailSetings = options.Value;
        }

        public async Task SendResetPasswordEmail(string resetPasswordEmailLink, string ToEmail)
        {
            try
            {
                var smptClient = new SmtpClient();

                smptClient.Host = _emailSetings.Host;

                smptClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smptClient.UseDefaultCredentials = false;
                smptClient.Port = 587;
                smptClient.Credentials = new NetworkCredential(_emailSetings.Email, _emailSetings.Password);
                smptClient.EnableSsl = true;

                var mailMessage = new MailMessage();

                mailMessage.From = new MailAddress(_emailSetings.Email);//mail kimden gidecek
                mailMessage.To.Add(ToEmail);

                mailMessage.Subject = "LocalHost | Şifre sıfırlama linki";
                mailMessage.Body = @$"<h4>Şifrenizi yenilemek için aşağıdaki linke tıklayınız.</h4><p><a href='{resetPasswordEmailLink}'>şifre yenileme link</a></p>";
                mailMessage.IsBodyHtml = true;

                await smptClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("E-posta gönderme hatası: " + ex.Message);

            }
        }
    }
}
