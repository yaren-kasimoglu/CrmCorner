using CrmCorner.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CrmCorner.Services.DailyReporting
{
    public class WeeklyDailyReportEmailService
    {
        private readonly CrmCornerContext _context;
        private readonly IConfiguration _configuration;

        public WeeklyDailyReportEmailService(
            CrmCornerContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SendLastWeekReportsAsync()
        {
            var today = DateTime.Today;

            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var thisWeekMonday = today.AddDays(-diff).Date;

            var lastWeekStart = thisWeekMonday.AddDays(-7);
            var lastWeekEnd = lastWeekStart.AddDays(4);

            var reports = await _context.DailyReports
                .AsNoTracking()
                .Where(x =>
                    x.ReportDate >= lastWeekStart &&
                    x.ReportDate <= lastWeekEnd)
                .ToListAsync();

            if (!reports.Any())
                return;

            var userIds = reports
                .Select(x => x.AppUserId)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(x => userIds.Contains(x.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                    continue;

                var userReports = reports
                    .Where(x => x.AppUserId == user.Id)
                    .ToList();

                if (!userReports.Any())
                    continue;

                var subject = $"Haftalık SDR Raporun - {lastWeekStart:dd.MM.yyyy} / {lastWeekEnd:dd.MM.yyyy}";
                var body = BuildEmailBody(user, userReports, lastWeekStart, lastWeekEnd);

                await SendEmailAsync(user.Email, subject, body);
            }
        }

        private string BuildEmailBody(
            AppUser user,
            List<DailyReport> reports,
            DateTime weekStart,
            DateTime weekEnd)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<div style='font-family:Arial,sans-serif;font-size:14px;color:#111827;'>");
            sb.AppendLine($"<h2>Merhaba {user.NameSurname},</h2>");
            sb.AppendLine($"<p><strong>{weekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}</strong> haftasına ait SDR raporun aşağıdadır.</p>");

            var companyGroups = reports
                .GroupBy(x => x.CompanyName)
                .OrderBy(x => x.Key);

            foreach (var companyGroup in companyGroups)
            {
                sb.AppendLine($"<h3 style='margin-top:24px;color:#1f2937;'>{companyGroup.Key}</h3>");

                sb.AppendLine("<table style='width:100%;border-collapse:collapse;margin-top:8px;'>");
                sb.AppendLine("<thead>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<th style='border:1px solid #d1d5db;padding:8px;background:#263aa8;color:white;text-align:left;'>Aktivite</th>");
                sb.AppendLine("<th style='border:1px solid #d1d5db;padding:8px;background:#263aa8;color:white;'>Hedef</th>");
                sb.AppendLine("<th style='border:1px solid #d1d5db;padding:8px;background:#263aa8;color:white;'>Gerçekleşen</th>");
                sb.AppendLine("<th style='border:1px solid #d1d5db;padding:8px;background:#263aa8;color:white;'>%</th>");
                sb.AppendLine("<th style='border:1px solid #d1d5db;padding:8px;background:#263aa8;color:white;'>GAP</th>");
                sb.AppendLine("</tr>");
                sb.AppendLine("</thead>");
                sb.AppendLine("<tbody>");

                var activityGroups = companyGroup
                    .GroupBy(x => x.ActivityType)
                    .OrderBy(x => x.Key);

                foreach (var activityGroup in activityGroups)
                {
                    var totalP = activityGroup.Sum(x => x.ProspectTarget);
                    var totalA = activityGroup.Sum(x => x.ActualValue);
                    var percent = totalP == 0 ? 0 : (int)Math.Round((decimal)totalA / totalP * 100);
                    var gap = totalA - totalP;

                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td style='border:1px solid #d1d5db;padding:8px;font-weight:bold;'>{activityGroup.Key}</td>");
                    sb.AppendLine($"<td style='border:1px solid #d1d5db;padding:8px;text-align:center;'>{totalP}</td>");
                    sb.AppendLine($"<td style='border:1px solid #d1d5db;padding:8px;text-align:center;'>{totalA}</td>");
                    sb.AppendLine($"<td style='border:1px solid #d1d5db;padding:8px;text-align:center;color:#ef4444;font-weight:bold;'>{percent}%</td>");
                    sb.AppendLine($"<td style='border:1px solid #d1d5db;padding:8px;text-align:center;color:#ef4444;font-weight:bold;'>{gap}</td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</tbody>");
                sb.AppendLine("</table>");
            }

            sb.AppendLine("<p style='margin-top:24px;color:#6b7280;'>Bu mail CRM Corner tarafından otomatik gönderilmiştir.</p>");
            sb.AppendLine("</div>");

            return sb.ToString();
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpHost = _configuration["SmtpSettings:Host"];
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
            var smtpUser = _configuration["SmtpSettings:Username"];
            var smtpPassword = _configuration["SmtpSettings:Password"];
            var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser, "CRM Corner"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}