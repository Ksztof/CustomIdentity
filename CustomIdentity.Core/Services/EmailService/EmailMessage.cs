using MimeKit;

namespace CustomIdentity.Core.Services.EmailService
{
	public abstract class EmailMessage : IEmailMessage
	{
		public abstract MimeMessage GetEmailMessage(string userRegistrationToken, string userEmail);
	}
}




