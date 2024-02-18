using System;
using System.Net;
using System.Net.Mail;

namespace CrmCorner.Models
{
	public class EmailSender :IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string message)
		{
			var mail = "oznurr03@gmail.com";
			var pw = "Ok7763498";
			var client = new SmtpClient("smtp-email.outlook.com", 587)
			{
				EnableSsl = true,
				Credentials = new NetworkCredential(mail, pw)

			};
			return client.SendMailAsync(new MailMessage(from: mail,
				to: email,
				subject: "test", "denem"));
		}

	}
}

