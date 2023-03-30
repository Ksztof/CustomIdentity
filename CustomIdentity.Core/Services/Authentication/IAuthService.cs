using CustomIdentity.Core.FormModels.IdentityModels;
using CustomIdentity.Core.HelperModels;
using CustomIdentity.Domain.DatabaseModels.Identities;
using System.IdentityModel.Tokens.Jwt;

namespace CustomIdentity.Core.Services.Authentication
{
	public interface IAuthService
	{
		public Task<ActionResultM<User>> SignUpAsync(SignUpM signUpM);
		public Task<ActionResultM<User>> CheckIfUserExistsAsync(SignInM signInM);
		public Task<ActionResultM<User>> VerifyTokenAsync(string verificationToken);
		public Task<ActionResultM<User>> RefreshRegistrationTokenAsync(string userEmail);
		public Task<ActionResultM<User>> GiveTokensAsync(SignInM signInM);
		public Task<ActionResultM<User>> LogoutAsync();
		public Task<ActionResultM<User>> DeleteAccountAsync();
		public Task<ActionResultM<IAsyncEnumerable<AccountsToDeleteM>>> GetDeletedAccountsAsync();
		public Task<ActionResultM<User>> ConfirmAccountDeletionAsync(int accountToDeleteId);
		public Task<ActionResultM<User>> UpdateLoginAndPasswordAsync(UpdateLoginAndPasswordM updateLoginAndPasswordM);
		public Task<ActionResultM<User>> GetUserAsync(int userId);
		public Task<ActionResultM<UserProfile>> EmailLoginCheck(SignUpM signUpM);
		public Task<ActionResultM<User>> GiveTokensAsync(User user);

	}
}
