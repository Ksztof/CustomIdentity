using MimeKit;

namespace CustomIdentity.Core.Services.EmailService
{
	public interface IEmailMessage
	{
		public MimeMessage GetEmailMessage(string userRegistrationToken, string userEmail);
	}
}
