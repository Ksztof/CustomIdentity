using MailKit.Net.Smtp;
using MailKit.Security;

namespace CustomIdentity.Core.Services.EmailService
{
	public class SmtpEmailService : EmailService
	{
		public override SmtpClient GetSmtpClient()
		{
			{
				var smtp = new SmtpClient();
				smtp.Connect("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);
				smtp.Authenticate("rowland.nienow@ethereal.email", "HYTMJPjCxVQJ9eNKXY");

				return smtp;
			}
		}
	}
}
