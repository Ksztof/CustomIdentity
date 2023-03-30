using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomIdentity.Core.FormModels.IdentityModels;
using CustomIdentity.Core.Services.Authentication;
using CustomIdentity.Core.HelperModels;
using CustomIdentity.Domain.DatabaseModels.Identities;

namespace CustomIdentity.API.Controllers
{
	[Route("/Auth")]
	[ApiController]
	public class AuthController : ControllerBase, IAuthController
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}



		[HttpPost]
		[Route("/Auth/SignUpAsync/")]
		public async Task<IActionResult> SignUpAsync([FromBody] SignUpM signUp)
		{
			ActionResultM<User> signIn = await _authService.SignUpAsync(signUp);
			if (signIn.Status != ActionStatus.Success)
			{
				return BadRequest(signIn.Message);
			}

			return Ok(signIn.Message);
		}


		[HttpGet]
		[Route("/Auth/VerifyRegistrationTokenAsync/{emailVerificationToken}/")]
		public async Task<IActionResult> VerifyRegistrationTokenAsync(string emailVerificationToken)
		{
			ActionResultM<User> verifyToken = await _authService.VerifyTokenAsync(emailVerificationToken);
			if (verifyToken.Status != ActionStatus.Success)
			{
				return BadRequest(verifyToken.Message);
			}

			return Ok(verifyToken.Message);
		}


		[HttpPost]
		[Route("/Auth/RefreshRegistrationTokenAsync/{userEmail}/")]
		public async Task<IActionResult> RefreshRegistrationTokenAsync(string userEmail)
		{
			ActionResultM<User> refreshToken = await _authService.RefreshRegistrationTokenAsync(userEmail);
			if (refreshToken.Status != ActionStatus.Success)
			{
				return NotFound(refreshToken.Message);
			}

			return Ok(refreshToken.Message);
		}


		[HttpPost]
		[Route("/Auth/SignInAsync/")]
		public async Task<IActionResult> SignInAsync([FromBody] SignInM signInM)
		{
			ActionResultM<User> assignTokens = await _authService.GiveTokensAsync(signInM);
			if (assignTokens.Status is not ActionStatus.Success)
			{
				return NotFound(assignTokens.Message);
			}

			return Ok();
		}


		[HttpPost]
		[Authorize]
		[Route("/Auth/LogoutAsync/")]
		public async Task<IActionResult> LogoutAsync()
		{
			ActionResultM<User> logout = await _authService.LogoutAsync();
			if (logout.Status != ActionStatus.Success)
			{
				return BadRequest(logout.Message);
			}

			return Ok("Logged out");
		}


		[HttpPost]
		[Authorize(Roles = "USER")]
		[Route("/Auth/UpdateLoginAndPasswordAsync/")]
		public async Task<IActionResult> UpdateLoginAndPasswordAsync([FromBody] UpdateLoginAndPasswordM updateLoginAndPasswordM)
		{
			ActionResultM<User> updateCredentials = await _authService.UpdateLoginAndPasswordAsync(updateLoginAndPasswordM);
			if (updateCredentials.Status is not ActionStatus.Success)
			{
				return NotFound(updateCredentials.Message);
			}

			return Ok("Account updated");
		}


		[HttpPost]
		[Authorize(Roles = "USER")]
		[Route("/Auth/DeleteAccountAsync/")]
		public async Task<IActionResult> DeleteAccountAsync()
		{
			ActionResultM<User> deleteAccount = await _authService.DeleteAccountAsync();
			if (deleteAccount.Status is not ActionStatus.Success)
			{
				return NotFound(deleteAccount.Message);
			}

			return Ok(deleteAccount.Message);
		}


		[HttpGet]
		[Authorize(Roles = "USER")]
		[Route("/Auth/ConfirmAccountDeletionAsync/")]
		public async Task<IActionResult> ConfirmAccountDeletionAsync()
		{
			ActionResultM<IAsyncEnumerable<AccountsToDeleteM>> getDeletedAcc = await _authService.GetDeletedAccountsAsync();
			if (getDeletedAcc.Status is not ActionStatus.Success)
			{
				return BadRequest(getDeletedAcc.Message);
			}

			return Ok(getDeletedAcc.Data);
		}


		[HttpPost]
		[Authorize(Roles = "USER")]
		[Route("/Auth/ConfirmAccountDeletionAsync/")]
		public async Task<IActionResult> ConfirmAccountDeletionAsync([FromBody] ChooseAccountToDeleteM chooseAccountToDeleteM)
		{
			ActionResultM<User> confirmDeletion = await _authService.ConfirmAccountDeletionAsync(chooseAccountToDeleteM.AccountId);
			if (confirmDeletion.Status is not ActionStatus.Success)
			{
				return BadRequest(confirmDeletion.Message);
			}

			return Ok("Account has been successfully deleted");
		}


	}
}

