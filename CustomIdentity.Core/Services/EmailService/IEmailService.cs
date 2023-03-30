using MailKit.Net.Smtp;

namespace CustomIdentity.Core.Services.EmailService
{
	public interface IEmailService
	{
		public SmtpClient GetSmtpClient();
	}
}
