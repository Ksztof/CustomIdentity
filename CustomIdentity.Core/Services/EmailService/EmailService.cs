using MailKit.Net.Smtp;

namespace CustomIdentity.Core.Services.EmailService
{
	public abstract class EmailService : IEmailService
	{
		public abstract SmtpClient GetSmtpClient();
	}
}
