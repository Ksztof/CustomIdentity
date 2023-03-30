using CustomIdentity.Core.HelperModels;
using CustomIdentity.Domain.DatabaseModels.Identities;
using System.IdentityModel.Tokens.Jwt;

namespace CustomIdentity.Core.TokenGenerator
{
	public interface ITokenGenerator
	{
		public JwtSecurityToken GenerateJwtTokenForRegistration(int userId);
		public Task<ActionResultM<JwtSecurityToken>> GenerateJwtAuthorizatioToken(User user);
		public Task<bool> ValidateRegistrationTokenAsync(JwtSecurityToken token);
		public int GetUserIdFromRegistrationToken(string jwtToken);
		public Task<ActionResultM<JwtSecurityToken>> GenerateJwtRefreshToken(User user);
		public string TokenToString(JwtSecurityToken token);
		public JwtSecurityToken DecodeToked(string token);
		public string GetUserRoleFromJwtToken(string jwtToken);
		public int GetUserIdFromAuthorizationToken(string jwtToken);
		public Task<bool> ValidateTokenAsync(string token);
		public Task<bool> ValidateFullTokenAsync(string token);
		public Task<bool> ValidateTokenLifeTimeAsync(string token);
		public DateTime GetTokenExpirationTime(string token);

	}
}
