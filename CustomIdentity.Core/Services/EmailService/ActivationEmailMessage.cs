using MimeKit;
using MimeKit.Text;

namespace CustomIdentity.Core.Services.EmailService
{
	public class ActivationEmailMessage : EmailMessage
	{
		public override MimeMessage GetEmailMessage(string userRegistrationToken, string userEmail)
		{
			var email = new MimeMessage();
			email.From.Add(MailboxAddress.Parse("rowland.nienow@ethereal.email"));
			email.To.Add(MailboxAddress.Parse(userEmail));
			email.Subject = "Activate your account";

			// HTML styles for various elements
			var grayBackground = "style=\"background-color: #F1F1F1; padding: 20px; text-align: center;\"";
			var boldText = "style=\"font-weight: bold; text-align: center;\"";
			var blueButton = "style=\"background-color: #0070C0; border-radius: 5px; display: inline-block; padding: 10px 0px; margin: 0 auto; width: 200px; text-align: center;\"";
			var buttonLink = "style=\"color: #FFF; text-decoration: none;\"";

			// HTML content for the email message
			var message = $"<div {boldText}>Thank you for choosing our services! To activate your account, click the button below:</div><br><div><a href=\"https://localhost:7064/Auth/VerifyRegistrationTokenAsync/{userRegistrationToken}\" {blueButton}><span {buttonLink}>Activate your account!</span></a></div>";

			// Wrap the message in a gray background
			var body = $"<div {grayBackground}>{message}</div>";

			email.Body = new TextPart(TextFormat.Html)
			{
				Text = body
			};

			return email;
		}
	}
}
