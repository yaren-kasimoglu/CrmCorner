using CrmCorner.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

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
            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            using var mailMessage = new MailMessage
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
            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
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

        public async Task SendTodoExpirationWarningAsync(string to, string taskText, DateTime? expiresAt)
        {
            var subject = "Görevinizin süresi dolmak üzere";

            var body = $@"
                <div style='font-family:Segoe UI,Arial,sans-serif; line-height:1.6; color:#333;'>
                    <h3 style='color:#d97706; margin-bottom:12px;'>Görev Uyarısı</h3>
                    <p>Merhaba,</p>
                    <p>
                        <strong>{taskText}</strong> başlıklı <strong>Günüm</strong> görevinizin süresi dolmak üzere.
                    </p>
                    <p>
                        Bu görev sistem tarafından otomatik olarak silinecektir.
                    </p>
                    <p>
                        <strong>Silinme zamanı:</strong> {(expiresAt.HasValue ? expiresAt.Value.ToString("dd.MM.yyyy HH:mm:ss") : "-")}
                    </p>
                    <hr style='border:none; border-top:1px solid #eee; margin:20px 0;' />
                    <p style='font-size:13px; color:#666;'>
                        CRM Corner otomatik bildirim mesajıdır.
                    </p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendTaskAssignedEmailAsync(string to, string assignedByName, string taskText, string taskUrl)
        {
            var subject = "Size yeni bir görev atandı";

            var body = $@"
        <div style='font-family:Segoe UI,Arial,sans-serif; line-height:1.6; color:#333;'>
            <h2 style='color:#7a4b00; margin-bottom:12px;'>CRM Corner Görev Bildirimi</h2>

            <p>Merhaba,</p>

            <p>
                <strong>{assignedByName}</strong> size CRM Corner üzerinden yeni bir görev atadı.
            </p>

            <div style='background:#fff8e6; border:1px solid #ffd36b; border-radius:12px; padding:14px; margin:16px 0;'>
                <div style='font-size:13px; color:#8a6a26; margin-bottom:6px;'>Atanan Görev</div>
                <div style='font-size:15px; font-weight:700; color:#333;'>{taskText}</div>
            </div>

            <p>
                Görevi görüntülemek için aşağıdaki butona tıklayabilirsiniz:
            </p>

            <p style='margin:20px 0;'>
                <a href='{taskUrl}'
                   style='display:inline-block; background:#ffb71b; color:#fff; text-decoration:none; padding:12px 18px; border-radius:10px; font-weight:700;'>
                    Görevi Görüntüle
                </a>
            </p>

            <p style='font-size:13px; color:#666; margin-top:24px;'>
                Bu mesaj CRM Corner tarafından otomatik olarak gönderilmiştir.
            </p>
        </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendTodoDeadlineReminderEmailAsync(
    string toEmail,
    string? userName,
    string taskText,
    string reminderType,
    DateTime deadline)
        {
            var subject = $"CRMCorner Görev Hatırlatması - {reminderType}";

            var safeUserName = string.IsNullOrWhiteSpace(userName) ? "Merhaba" : userName;

            var body = $@"
    <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
        <div style='max-width: 640px; margin: 0 auto; background: #fffaf0; border: 1px solid #ffe0a3; border-radius: 16px; overflow: hidden;'>
            
            <div style='background: linear-gradient(135deg, #ffb71b, #f2a800); padding: 20px 24px; color: white;'>
                <h2 style='margin: 0; font-size: 22px;'>Görev Hatırlatması</h2>
            </div>

            <div style='padding: 24px;'>
                <p style='margin-top: 0;'>Merhaba {safeUserName},</p>

                <p>
                    <strong>{taskText}</strong> başlıklı görevin son tarihine
                    <strong>{reminderType}</strong>.
                </p>

                <div style='background: #fff8e6; border: 1px solid #ffd36b; border-radius: 12px; padding: 14px 16px; margin: 18px 0;'>
                    <p style='margin: 0;'><strong>Görev:</strong> {taskText}</p>
                    <p style='margin: 8px 0 0 0;'><strong>Son Tarih:</strong> {deadline:dd.MM.yyyy HH:mm}</p>
                </div>

                <p>
                    Lütfen CRMCorner üzerinden görevi kontrol edin.
                </p>

                <p style='margin-bottom: 0;'>
                    İyi çalışmalar,<br />
                    <strong>CRMCorner</strong>
                </p>
            </div>
        </div>
    </div>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}