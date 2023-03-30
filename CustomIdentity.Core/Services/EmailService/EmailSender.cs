using CustomIdentity.Core.HelperModels;
using MailKit.Net.Smtp;
using System.Text;

namespace CustomIdentity.Core.Services.EmailService
{
	public class EmailSender : IEmailSender
	{
		private readonly IEmailService _emailService;
		private readonly IEmailMessage _emailMessage;

		public EmailSender(IEmailService emailService, IEmailMessage emailMessage)
		{
			_emailService = emailService;
			_emailMessage = emailMessage;
		}


		public async Task<ActionResultM<SmtpClient>> SendEmailAsync(string userRegistrationToken, string userEmail)
		{
			try
			{
				var email = _emailMessage.GetEmailMessage(userRegistrationToken, userEmail);
				var smtp = _emailService.GetSmtpClient();
				var response = await smtp.SendAsync(email);
				var responseResult = response.Split(" ")[0];
				await smtp.DisconnectAsync(true);
				if (responseResult != "Accepted")
				{
					return new ActionResultM<SmtpClient>("Can't send email, please check your email.");
				}

				Console.WriteLine(response[0]);
				return new ActionResultM<SmtpClient>("OK", response);
			}
			catch (Exception e)
			{
				return new ActionResultM<SmtpClient>(e.Message);
			}
		}


		public static string HashToken(string token)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
		}


	}
}
