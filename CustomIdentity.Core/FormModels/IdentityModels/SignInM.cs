using System.ComponentModel.DataAnnotations;

namespace CustomIdentity.Core.FormModels.IdentityModels
{
	public class SignInM
	{
		[Required]
		[RegularExpression(@"^[a-zA-Z]{2,}$", ErrorMessage = "Enter your login again, login should consist of at least two letters")]
		public string Login { get; set; } = string.Empty;

		[Required]
		[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{7,}$", ErrorMessage = "Enter your password again, password should consist of at least seven characters, one capital letter, one special character and one number")]
		public string Password { get; set; } = string.Empty;
	}
}
