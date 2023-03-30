using CustomIdentity.Core.HelperModels;
using MailKit.Net.Smtp;

namespace CustomIdentity.Core.Services.EmailService
{
	public interface IEmailSender
	{
		public Task<ActionResultM<SmtpClient>> SendEmailAsync(string userRegistrationToken, string userEmail);
	}
}
