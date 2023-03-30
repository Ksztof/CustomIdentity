using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomIdentity.Core.FormModels.IdentityModels
{
	public class UpdateLoginAndPasswordM
	{
		[Required]
		[RegularExpression(@"^[a-zA-Z]{2,}$", ErrorMessage = "Enter your login again, login should consist of at least two letters")]
		public string? ChangedLogin { get; set; }

		[Required]
		[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{7,}$", ErrorMessage = "Enter your password again, password should consist of at least seven characters, one capital letter, one special character and one number")]
		public string? ChangedPassword { get; set; }

		[Required]
		[RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{7,}$", ErrorMessage = "Enter your password again, password should consist of at least seven characters, one capital letter, one special character and one number")]
		public string? ConfirmChangedPassword { get; set; }

	}
}
