using CustomIdentity.Core.FormModels.IdentityModels;
using CustomIdentity.Core.HelperModels;
using CustomIdentity.Core.Repositories.User;
using CustomIdentity.Core.Services.EmailService;
using CustomIdentity.Core.TokenGenerator;
using CustomIdentity.Core.Utility;
using CustomIdentity.Domain.DatabaseModels.Identities;
using CustomIdentity.Domain.UnitOfWork;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;


namespace CustomIdentity.Core.Services.Authentication
{
	public class AuthService : IAuthService
	{
		private readonly IEmailSender _emailSender;
		private readonly IUtilityClass _utilityClass;
		private readonly ITokenGenerator _tokenGenerator;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<AuthService> _logger;


		private UserRepository userRepository = null;
		private UserRepository _userRepository
		{
			get
			{
				if (userRepository == null)
				{
					userRepository = new UserRepository(_unitOfWork, _utilityClass);
				}

				return userRepository;
			}
		}


		public AuthService(IEmailSender emailSender, IUtilityClass utilityClass, ITokenGenerator tokenGenerator, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
		{
			_emailSender = emailSender;
			_utilityClass = utilityClass;
			_tokenGenerator = tokenGenerator;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
		}



		public async Task<ActionResultM<User>> SignUpAsync(SignUpM signUpM)
		{
			try
			{
				ActionResultM<SignUpM> checkForm = _utilityClass.CheckSignUpForm(signUpM);
				if (checkForm.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(checkForm.Message);
				}

				ActionResultM<UserProfile> checkAccountExist = await EmailLoginCheck(signUpM);
				if (checkAccountExist.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(checkAccountExist.Message);
				}

				_utilityClass.CreatePasswordHash(signUpM.Password, out byte[] passwordHash, out byte[] passwordSalt);

				UserProfile userProfile = new UserProfile
				{
					Login = signUpM.Login,
					FirstName = signUpM.FirstName.ToUpper(),
					LastName = signUpM.LastName.ToUpper(),
					Email = signUpM.Email.ToUpper(),
					PhoneNumber = signUpM.PhoneNumber,
					IsOnline = false
				};
				ActionResultM<UserProfile> addProfile = await _userRepository.AddUserProfileAsync(userProfile);
				if (addProfile.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(addProfile.Message);
				}

				UserCredentials userCredential = new UserCredentials
				{
					PasswordHash = passwordHash,
					PasswordSalt = passwordSalt,
					CredentialName = "BASIC",
					UserAuthMethodId = UserAuthMethod.Basic,
					Tokens = new Tokens
					{
					}
				};
				ActionResultM<UserCredentials> addCredentials = await _userRepository.AddUserCredentialsAsync(userCredential);
				if (addCredentials.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(addCredentials.Message);
				}

				UserPermissions userPermissions = new UserPermissions { };
				ActionResultM<UserPermissions> addPermissions = await _userRepository.AddUserPermissionsAsync(userPermissions);
				if (addPermissions.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(addPermissions.Message);
				}

				var user = new User
				{
					UserProfileId = userProfile.UserProfileId,
					UserProfile = userProfile,
					UserCredentialsId = userCredential.UserCredentialsId,
					Credentials = userCredential,
					PermissionsId = userPermissions.UserPermissionsId,
					UserPermissions = userPermissions
				};
				ActionResultM<User> addCompleteUser = await _userRepository.AddAsync(user);
				if (addCompleteUser.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(addCompleteUser.Message);
				}

				user = addCompleteUser.Data;
				JwtSecurityToken newRegTok = _tokenGenerator.GenerateJwtTokenForRegistration(user.Id);
				string newRegTokStr = _tokenGenerator.TokenToString(newRegTok);

				user.Credentials.Tokens.RegistrationJwtToken = newRegTokStr;

				ActionResultM<User> updateRegTok = await _userRepository.UpdateAsync(user);
				if (updateRegTok.Status != ActionStatus.Success || updateRegTok is null)
				{
					return new ActionResultM<User>(updateRegTok.Message);
				}

				user = updateRegTok.Data;
				ActionResultM<SmtpClient> sendingEmailResult = await _emailSender.SendEmailAsync(newRegTokStr, userProfile.Email);
				if (sendingEmailResult.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(sendingEmailResult.Message);
				}
				return new ActionResultM<User>(user);
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}
		}


		public async Task<ActionResultM<User>> CheckIfUserExistsAsync(SignInM signInM)
		{
			try
			{
				ActionResultM<SignInM> checkForm = _utilityClass.CheckSignInForm(signInM);
				if (checkForm.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(checkForm.Message);
				}

				ActionResultM<User> checkifExist = await _userRepository.IsLoginAvailable(signInM.Login);
				if (checkifExist.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(checkifExist.Message);
				}

				User user = checkifExist.Data;
				byte[] pswdHash = user.Credentials.PasswordHash;
				byte[] pswdSalt = user.Credentials.PasswordSalt;
				bool verifyPswd = _utilityClass.VerifyPasswordHash(signInM.Password, pswdHash, pswdSalt);
				if (!verifyPswd)
				{
					return new ActionResultM<User>("Wrong Credentials");
				}

				return new ActionResultM<User>(user);
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}
		}


		public async Task<ActionResultM<User>> VerifyTokenAsync(string token)
		{
			try
			{
				ActionResultM<User> getUser = await _userRepository.GetUserByTokenAsync(token);
				if (getUser.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(getUser.Message);
				}

				User user = getUser.Data;
				int useIdTok = _tokenGenerator.GetUserIdFromRegistrationToken(token);
				var userId = user.Id;
				if (useIdTok == userId)
				{
					JwtSecurityToken decodedTok = _tokenGenerator.DecodeToked(token);
					bool tokenValidated = await _tokenGenerator.ValidateRegistrationTokenAsync(decodedTok);
					if (!tokenValidated)
					{
						string newJwtRegistrationTokenString = _tokenGenerator.TokenToString(_tokenGenerator.GenerateJwtTokenForRegistration(userId));
						string userEmail = user.UserProfile.Email;

						user.Credentials.Tokens.RegistrationJwtToken = newJwtRegistrationTokenString;

						ActionResultM<User> assignRefrTok = await _userRepository.UpdateAsync(user);
						if (assignRefrTok.Status != ActionStatus.Success || assignRefrTok.Data is null)
						{
							return new ActionResultM<User>(assignRefrTok.Message);
						}

						ActionResultM<SmtpClient> resendActivLink = await _emailSender.SendEmailAsync(newJwtRegistrationTokenString, userEmail);
						if (resendActivLink.Status != ActionStatus.Success)
						{
							return new ActionResultM<User>(resendActivLink.Message);
						}

						return new ActionResultM<User>("NOTOK", "Token is not valid or has expired, check your mailbox - we sent new token");
					}

					user.Credentials.AccountVerified = true;
					user.Credentials.Tokens.RegistrationJwtToken = "USED";
					ActionResultM<User> activateUser = await _userRepository.UpdateAsync(user);
					if (activateUser.Status != ActionStatus.Success)
					{
						return new ActionResultM<User>(activateUser.Message);
					}

					int permissionsId = activateUser.Data.PermissionsId;
					ActionResultM<UserRoles> giveRole = await _userRepository.AddUserRoleAsync(permissionsId);
					if (giveRole.Status is not ActionStatus.Success)
					{
						return new ActionResultM<User>(giveRole.Message);
					}

					return new ActionResultM<User>("OK", "Account has been activated.");
				}

				return new ActionResultM<User>("NOTOK", "Wrong token");
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}
		}


		public async Task<ActionResultM<User>> RefreshRegistrationTokenAsync(string userEmail)
		{
			try
			{
				ActionResultM<User> getUser = await _userRepository.GetUserByMailAsync(userEmail);
				if (getUser.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(getUser.Message);
				}

				User user = getUser.Data;
				bool isAccountVerified = user.Credentials.AccountVerified;
				int userId = user.Id;

				string newRegToken = _tokenGenerator.TokenToString(_tokenGenerator.GenerateJwtTokenForRegistration(userId));
				if (isAccountVerified is true)
				{
					return new ActionResultM<User>("account is already active, you can log in.");
				}

				user.Credentials.Tokens.RegistrationJwtToken = newRegToken;

				ActionResultM<User> assignNewRegTok = await _userRepository.UpdateAsync(user);
				if (assignNewRegTok.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(assignNewRegTok.Message);
				}

				ActionResultM<SmtpClient> sendRegistrationTokenResult = await _emailSender.SendEmailAsync(newRegToken, userEmail);
				if (sendRegistrationTokenResult.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(sendRegistrationTokenResult.Message);
				}

				return new ActionResultM<User>("OK", "e-mail with activation link has been resent, please check your mailbox");
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}

		}


		public async Task<ActionResultM<User>> GiveTokensAsync(SignInM signInM)
		{
			try
			{
				ActionResultM<User> checkUserExists = await CheckIfUserExistsAsync(signInM);
				if (checkUserExists.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(checkUserExists.Message);
				}

				User user = checkUserExists.Data;

				ActionResultM<JwtSecurityToken> genAuthT = await _tokenGenerator.GenerateJwtAuthorizatioToken(user);
				if (genAuthT.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(genAuthT.Message);
				}

				ActionResultM<JwtSecurityToken> genReftT = await _tokenGenerator.GenerateJwtRefreshToken(user);
				if (genReftT.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(genReftT.Message);
				}

				JwtSecurityToken authT = genAuthT.Data;
				JwtSecurityToken refrT = genReftT.Data;
				string authorizationTokenString = _tokenGenerator.TokenToString(authT);
				string refreshTokenString = _tokenGenerator.TokenToString(refrT);
				user.Credentials.Tokens.RefreshJwtToken = refreshTokenString;

				ActionResultM<User> saveRefreshT = await _userRepository.UpdateAsync(user);
				if (saveRefreshT.Status != ActionStatus.Success || saveRefreshT.Data is null)
				{
					return new ActionResultM<User>(saveRefreshT.Message);
				}

				_httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenString, new CookieOptions
				{
					HttpOnly = true,
					SameSite = SameSiteMode.Strict,
					Expires = refrT.ValidTo,
					Secure = true
				});

				_httpContextAccessor.HttpContext.Response.Headers.Add("Authorization", "Bearer " + authorizationTokenString);

				return new ActionResultM<User>(saveRefreshT.Data);
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}
		}


		public async Task<ActionResultM<User>> GiveTokensAsync(User user)
		{
			try
			{
				ActionResultM<JwtSecurityToken> genAuthT = await _tokenGenerator.GenerateJwtAuthorizatioToken(user);
				if (genAuthT.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(genAuthT.Message);
				}

				ActionResultM<JwtSecurityToken> genRefrT = await _tokenGenerator.GenerateJwtRefreshToken(user);
				if (genRefrT.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(genRefrT.Message);
				}

				JwtSecurityToken refrToken = genRefrT.Data;
				JwtSecurityToken authToken = genAuthT.Data;
				string authTokenStr = _tokenGenerator.TokenToString(authToken);
				string refrTokenStr = _tokenGenerator.TokenToString(refrToken);

				user.Credentials.Tokens.RefreshJwtToken = refrTokenStr;

				ActionResultM<User> giveRefrTok = await _userRepository.UpdateAsync(user);
				if (giveRefrTok.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(giveRefrTok.Message);
				}

				_httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refrTokenStr, new CookieOptions
				{
					HttpOnly = true,
					SameSite = SameSiteMode.Strict,
					Expires = refrToken.ValidTo,
					Secure = true
				});

				_httpContextAccessor.HttpContext.Response.Headers.Add("Authorization", "Bearer " + authTokenStr);

				return new ActionResultM<User>(giveRefrTok.Data);
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}
		}


		public async Task<ActionResultM<User>> LogoutAsync()
		{
			try
			{
				string? refreshToken = _httpContextAccessor?.HttpContext?.Request.Cookies["refreshToken"];
				if (refreshToken is null)
				{
					return new ActionResultM<User>("Token doesn't exist, can't logout");
				}

				_httpContextAccessor?.HttpContext?.Response.Cookies.Delete("refreshToken", new CookieOptions
				{
					//Expires = DateTime.UtcNow.AddDays(-1),
					HttpOnly = true,
					SameSite = SameSiteMode.Strict,
					Secure = true,
					Path = "/"
				});

				ActionResultM<Tokens> deleteRefrTok = await _userRepository.SetRefreshTokenNullAsync(refreshToken);
				if (deleteRefrTok.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(deleteRefrTok.Message);
				}

				_httpContextAccessor?.HttpContext?.Response.Headers.Remove("Authorization");

				return new ActionResultM<User>("OK", "Logged out ");
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}
		}


		public async Task<ActionResultM<User>> DeleteAccountAsync()
		{
			try
			{
				StringValues? authorizationToken = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"];
				if (authorizationToken is null)
				{
					return new ActionResultM<User>("Can't find authorization token");
				}

				int userId = _tokenGenerator.GetUserIdFromAuthorizationToken(authorizationToken);
				ActionResultM<User> markAsdeleted = await _userRepository.MarkAccountDeletedAsync(userId);
				if (markAsdeleted.Status is not ActionStatus.Success || markAsdeleted.Data is null)
				{
					return new ActionResultM<User>(markAsdeleted.Message);
				}

				User user = markAsdeleted.Data;

				await LogoutAsync();
				return new ActionResultM<User>(user);
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}
		}


		public async Task<ActionResultM<IAsyncEnumerable<AccountsToDeleteM>>> GetDeletedAccountsAsync()
		{
			try
			{
				ActionResultM<IAsyncEnumerable<User>> getAccounts = await _userRepository.GetDeletedAccountsAsync();
				if (getAccounts.Status is not ActionStatus.Success)
				{
					return new ActionResultM<IAsyncEnumerable<AccountsToDeleteM>>(getAccounts.Message);
				}

				IAsyncEnumerable<User> accountsToDele = getAccounts.Data;
				IAsyncEnumerable<AccountsToDeleteM> formatAccounts = accountsToDele.Select(x => new AccountsToDeleteM
				{
					UserId = x.Id,
					UserProfileId = x.UserProfileId,
					UserName = x.UserProfile.FirstName,
					UserEmail = x.UserProfile.Email
				});

				return new ActionResultM<IAsyncEnumerable<AccountsToDeleteM>>(formatAccounts);
			}
			catch (Exception e)
			{
				return new ActionResultM<IAsyncEnumerable<AccountsToDeleteM>>(e);
			}

		}


		public async Task<ActionResultM<User>> ConfirmAccountDeletionAsync(int accountToDeleteId)
		{
			try
			{
				ActionResultM<User> accountDeletionResult = await _userRepository.confDelet(accountToDeleteId);
				if (accountDeletionResult.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(accountDeletionResult.Message);
				}

				User user = accountDeletionResult.Data;

				return new ActionResultM<User>(user);
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}

		}


		public async Task<ActionResultM<User>> UpdateLoginAndPasswordAsync(UpdateLoginAndPasswordM updateLoginAndPasswordM)
		{
			try
			{
				StringValues? authorizationToken = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"];
				if (authorizationToken is null)
				{
					return new ActionResultM<User>("Can't find authorization token");
				}

				int userId = _tokenGenerator.GetUserIdFromAuthorizationToken(authorizationToken);
				ActionResultM<User> getUser = await _userRepository.GetByIdAsync(userId);
				if (getUser.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>(getUser.Data);
				}

				User user = getUser.Data;
				string changedLogin = updateLoginAndPasswordM.ChangedLogin;
				string newPswd = updateLoginAndPasswordM.ChangedPassword;
				string confirmNewPswd = updateLoginAndPasswordM.ConfirmChangedPassword;
				if (newPswd != confirmNewPswd)
				{
					return new ActionResultM<User>("passwords do not match");
				}

				ActionResultM<User> findUser = await _userRepository.IsLoginAvailable(updateLoginAndPasswordM.ChangedLogin);
				if (findUser.Status is ActionStatus.Success && findUser.Data.UserProfile.Login != user.UserProfile.Login)
				{
					return new ActionResultM<User>("User with given login already exists");
				}

				_utilityClass.CreatePasswordHash(newPswd, out byte[] passwordHash, out byte[] passwordSalt);
				user.UserProfile.Login = changedLogin;
				user.Credentials.PasswordHash = passwordHash;
				user.Credentials.PasswordSalt = passwordSalt;

				ActionResultM<User> updateUser = await GiveTokensAsync(user);
				if (updateUser.Status is not ActionStatus.Success)
				{
					return new ActionResultM<User>("Can't update user");
				}

				return new ActionResultM<User>(updateUser.Data);
			}
			catch (Exception e)
			{
				return new ActionResultM<User>(e);
			}

		}


		public async Task<ActionResultM<User>> GetUserAsync(int userId)
		{
			try
			{
				ActionResultM<User> getUser = await _userRepository.GetByIdAsync(userId);
				if (getUser.Status != ActionStatus.Success)
				{
					return new ActionResultM<User>(getUser.Message);
				}

				User user = getUser.Data;
				return new ActionResultM<User>(user);
			}
			catch (Exception e)
			{ 
				return new ActionResultM<User>(e);
			}
		}


		public async Task<ActionResultM<UserProfile>> EmailLoginCheck(SignUpM signUpM)
		{
			try
			{
				ActionResultM<UserProfile> check = await _userRepository.EmailLoginCheck(signUpM);
				if (check.Status is not ActionStatus.Success)
				{
					return new ActionResultM<UserProfile>(check.Message);
				}

				return new ActionResultM<UserProfile>("OK", check.Message);
			}
			catch (Exception e)
			{
				return new ActionResultM<UserProfile>(e);
			}
		}


	}
}

