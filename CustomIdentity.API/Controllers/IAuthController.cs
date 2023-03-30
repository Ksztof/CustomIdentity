using CustomIdentity.Core.FormModels.IdentityModels;
using Microsoft.AspNetCore.Mvc;

namespace CustomIdentity.API.Controllers
{
	public interface IAuthController
	{
		public Task<IActionResult> SignUpAsync([FromBody] SignUpM signUp);
		public Task<IActionResult> VerifyRegistrationTokenAsync(string emailVerificationToken);
		public Task<IActionResult> SignInAsync([FromBody] SignInM signInM);
		public Task<IActionResult> RefreshRegistrationTokenAsync(string userEmail);
		public Task<IActionResult> LogoutAsync();
		public Task<IActionResult> DeleteAccountAsync();
		public Task<IActionResult> ConfirmAccountDeletionAsync();
		public Task<IActionResult> ConfirmAccountDeletionAsync([FromBody] ChooseAccountToDeleteM chooseAccountToDelete);
		public Task<IActionResult> UpdateLoginAndPasswordAsync([FromBody] UpdateLoginAndPasswordM updateLoginAndPasswordM);
	}
}
