using System.ComponentModel.DataAnnotations;

namespace CustomIdentity.Core.FormModels.IdentityModels
{
	public class SignUpM
	{
		[Required]
		[RegularExpression(@"^[a-zA-Z]{2,}$", ErrorMessage = "Enter your login again, login should consist of at least two letters")]
		public string Login { get; set; } = "";

		[Required]
		[RegularExpression(@"^(?=.*[A-Z])[a-zA-Z]{2,}$", ErrorMessage = "Enter your first name again, first name should consist of at least two letters and contain one capital letter")]
		public string FirstName { get; set; } = string.Empty;

		[Required]
		[RegularExpression(@"^(?=.*[A-Z])[a-zA-Z]{2,}$", ErrorMessage = "Enter your last name again, last name should consist of at least two letters and contain one capital letter")]
		public string LastName { get; set; } = string.Empty;

		[Required]
		[RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Enter your email address again, email should look like: example@example.com")]
		public string Email { get; set; } = string.Empty;

		[Required]
		[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{7,}$", ErrorMessage = "Enter your password again, password should consist of at least seven characters, one capital letter, one special character and one number")]
		public string Password { get; set; } = string.Empty;

		[Required]
		[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{7,}$", ErrorMessage = "Enter your password check again, password check should be same as your password, consist of at least seven characters, one capital letter, one special character and one number")]
		public string PasswordCheck { get; set; } = string.Empty;

		[Required]
		[RegularExpression(@"^\+(?:\d{1,3})?(?:\d{6,14})\d$", ErrorMessage = "Enter your phone number including country code, for example +48123123123")]
		public string PhoneNumber { get; set; } = string.Empty;
	}

}
