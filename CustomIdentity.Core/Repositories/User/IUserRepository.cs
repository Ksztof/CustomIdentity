using CustomIdentity.Core.FormModels.IdentityModels;
using CustomIdentity.Core.HelperModels;
using CustomIdentity.Domain.DatabaseModels.Identities;
using UserM = CustomIdentity.Domain.DatabaseModels.Identities.User;



namespace CustomIdentity.Core.Repositories.User
{
	public interface IUserRepository : IRepository<UserM>
	{
		public Task<ActionResultM<UserRoles>> AddUserRoleAsync(int? userPermissionsId);
		public Task<ActionResultM<UserM>> IsLoginAvailable(string login);
		public Task<ActionResultM<UserM>> GetUserByTokenAsync(string token);
		public Task<ActionResultM<IAsyncEnumerable<string>>> GetUserRolesAsync(int? userPermissionsId);
		public Task<ActionResultM<UserM>> GetUserByMailAsync(string userEmail);
		public Task<ActionResultM<Tokens>> SetRefreshTokenNullAsync(string refreshToken);
		public Task<ActionResultM<UserM>> MarkAccountDeletedAsync(int userId);
		public Task<ActionResultM<IAsyncEnumerable<UserM>>> GetDeletedAccountsAsync();
		public Task<ActionResultM<UserM>> confDelet(int accountToDeleteId);
		public Task<ActionResultM<UserM>> GetFullUserByIdAsync(int id);
		public Task<ActionResultM<UserProfile>> AddUserProfileAsync(UserProfile userProfile);
		public Task<ActionResultM<UserCredentials>> AddUserCredentialsAsync(UserCredentials userCredentials);
		public Task<ActionResultM<UserPermissions>> AddUserPermissionsAsync(UserPermissions userPermissions);
		public Task<ActionResultM<UserProfile>> EmailLoginCheck(SignUpM signUpM);


	}
}
