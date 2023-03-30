using CustomIdentity.Core.HelperModels;
using CustomIdentity.Core.Repositories.User;
using CustomIdentity.Domain.DatabaseModels.Identities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomIdentity.Core.TokenGenerator
{
	public class TokenGenerator : ITokenGenerator
	{
		private readonly IConfiguration _configuration;
		private readonly IUserRepository _userRepository;

		/*	private static JwtSecurityTokenHandler _handler => new JwtSecurityTokenHandler(); */


		public TokenGenerator(IConfiguration configuration, IUserRepository userRepository)
		{
			_configuration = configuration;
			_userRepository = userRepository;
		}

		public JwtSecurityToken GenerateJwtTokenForRegistration(int userId)
		{
			SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			JwtSecurityToken token = new JwtSecurityToken(
			_configuration["Jwt:Issuer"],
			_configuration["Jwt:Audience"],
			claims: new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
			expires: DateTime.UtcNow.AddMinutes(1),
			signingCredentials: credentials
		);
			return token;
		}


		public async Task<ActionResultM<JwtSecurityToken>> GenerateJwtAuthorizatioToken(User user)
		{
			try
			{
				SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
				var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
				var getRoles = await _userRepository.GetUserRolesAsync(user.PermissionsId);
				if (getRoles.Status is not ActionStatus.Success)
				{
					return new ActionResultM<JwtSecurityToken>(getRoles.Message);
				}

				var claims = new List<Claim>
				{
				new Claim(ClaimTypes.NameIdentifier, user.UserProfile.Login),
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				};

				await foreach (var role in getRoles.Data)
				{
					claims.Add(new Claim(ClaimTypes.Role, role));
				}

				var token = new JwtSecurityToken(
					_configuration["Jwt:Issuer"],
					_configuration["Jwt:Audience"],
					claims,
					expires: DateTime.UtcNow.AddMinutes(1),
					signingCredentials: credentials);

				return new ActionResultM<JwtSecurityToken>(token);
			}
			catch (Exception e)
			{
				return new ActionResultM<JwtSecurityToken>(e);
			}

		}


		public async Task<ActionResultM<JwtSecurityToken>> GenerateJwtRefreshToken(User user)
		{
			try
			{
				var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
				var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
				var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.UserProfile.Login) };

				var token = new JwtSecurityToken(
					_configuration["Jwt:Issuer"],
					_configuration["Jwt:Audience"],
					claims,
					expires: DateTime.UtcNow.AddMinutes(2),
					signingCredentials: credentials);

				return new ActionResultM<JwtSecurityToken>(token);
			}
			catch (Exception e)
			{
				return new ActionResultM<JwtSecurityToken>(e);
			}

		}


		public async Task<bool> ValidateRegistrationTokenAsync(JwtSecurityToken token)
		{
			try
			{
				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
				var tokenHandler = new JwtSecurityTokenHandler();
				var tokenString = tokenHandler.WriteToken(token);

				var validationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero,
					IssuerSigningKey = key
				};

				var principal = await tokenHandler.ValidateTokenAsync(tokenString, validationParameters);
				//var jwtToken = tokenHandler.ReadJwtToken(tokenString);
				var exp = token.Payload.Exp;
				if (exp == null || !long.TryParse(exp.ToString(), out var expTime))
				{
					return false;
				}

				return DateTimeOffset.FromUnixTimeSeconds(expTime).UtcDateTime >= DateTime.UtcNow;
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}

		}

		public async Task<bool> ValidateTokenAsync(string token)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			var decodeToken = DecodeToked(token);
			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var validatedToken = await tokenHandler.ValidateTokenAsync(decodeToken.RawData, new TokenValidationParameters
				{
					ValidateLifetime = false,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = key,
					ValidateIssuer = false,
					ValidateAudience = false,
				});
				return true;
			}
			catch
			{
				return false;
			}

		}


		public async Task<bool> ValidateFullTokenAsync(string token)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
				var validationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero,
					IssuerSigningKey = key
				};

				var principal = await tokenHandler.ValidateTokenAsync(token, validationParameters);
				var jwtToken = tokenHandler.ReadJwtToken(token);
				var exp = jwtToken.Payload.Exp;
				if (exp == null || !long.TryParse(exp.ToString(), out var expTime))
				{
					return false;
				}

				return DateTimeOffset.FromUnixTimeSeconds(expTime).UtcDateTime >= DateTime.UtcNow;
			}
			catch
			{
				return false;
			}

		}


		public string TokenToString(JwtSecurityToken token)
		{
			try
			{
				var tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);
				return tokenAsString;
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			
		}


		public JwtSecurityToken DecodeToked(string token)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var jwtToken = tokenHandler.ReadJwtToken(token);

				return jwtToken;
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			
		}


		public int GetUserIdFromRegistrationToken(string jwtToken)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var decodedToken = tokenHandler.ReadJwtToken(jwtToken);
				var userIdClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

				if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
				{
					return userId;
				}
				throw new ArgumentException("Missing or incorrect claim");
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			
		}


		public int GetUserIdFromAuthorizationToken(string authorizationToken)
		{
			try
			{
				string tokenValue = authorizationToken.Replace("Bearer ", "");

				var tokenHandler = new JwtSecurityTokenHandler();
				var decodedToken = tokenHandler.ReadJwtToken(tokenValue);
				var userIdClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out int userId));

				if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
				{
					return userId;
				}
				else
				{
					throw new ArgumentException("Missing or incorrect claim.");
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			
		}

		public string GetUserRoleFromJwtToken(string jwtToken)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var decodedToken = tokenHandler.ReadJwtToken(jwtToken);
				var roleClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

				if (roleClaim != null)
				{
					return roleClaim.Value;
				}
				throw new ArgumentException("Missing or incorrect claim");
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			
		}


		public async Task<bool> ValidateTokenLifeTimeAsync(string token)
		{
			try
			{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = false,
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero,
				IssuerSigningKey = key
			};
				var principal = await tokenHandler.ValidateTokenAsync(token, validationParameters);
				var jwtToken = tokenHandler.ReadJwtToken(token);
				var exp = jwtToken.Payload.Exp;
				if (exp == null || !long.TryParse(exp.ToString(), out var expTime))
				{
					return false;
				}

				return DateTimeOffset.FromUnixTimeSeconds(expTime).UtcDateTime >= DateTime.UtcNow;
			}
			catch
			{
				return false;
			}

		}


		public DateTime GetTokenExpirationTime(string token)
		{
			try
			{
				var handler = new JwtSecurityTokenHandler();
				var jwtToken = handler.ReadJwtToken(token);

				var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp");

				var expirationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
					.AddSeconds(double.Parse(expClaim.Value))
					.ToLocalTime();

				return expirationTime;
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
			
		}

	}
}
