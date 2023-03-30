using CustomIdentity.Core.FormModels.IdentityModels;
using CustomIdentity.Core.HelperModels;
using CustomIdentity.Core.Utility;
using CustomIdentity.Domain.DatabaseModels.Identities;
using CustomIdentity.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using UserM = CustomIdentity.Domain.DatabaseModels.Identities.User;

namespace CustomIdentity.Core.Repositories.User
{
	public class UserRepository : Repository, IUserRepository
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUtilityClass _utilityClass;
		public UserRepository(IUnitOfWork unitOfWork, IUtilityClass utilityClass)
		{
			_unitOfWork = unitOfWork;
			_utilityClass = utilityClass;
		}


		public async Task<ActionResultM<UserM>> AddAsync(UserM entity)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				EntityEntry<UserM> addUser = Db.Users.Add(entity);
				var AddedRows = await Db.SaveChangesAsync();
				if (/*addUser.State is not EntityState.Added ||*/ AddedRows <= 0)//TODO: EntityState = unchanged
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<UserM>("Can't add User");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<UserM>(addUser.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<UserM>(e.InnerException);
			}
		}


		public async Task<ActionResultM<UserM>> UpdateAsync(UserM entity)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				EntityEntry<UserM> updateUser = Db.Users.Update(entity);
				var updatedRows = await Db.SaveChangesAsync();
				if (/*updateUser.State is not EntityState.Modified ||*/updatedRows <= 0)//TODO: EntityState
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<UserM>("Update failed");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<UserM>(updateUser.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<UserM>(e);
			}
		}


		public async Task<ActionResultM<UserRoles>> AddUserRoleAsync(int? userPermissionsId)
		{
			try
			{
				UserRoles UserRole = new UserRoles { WebAppRoleId = (int)RolesValue.USER, UserPermissionsId = userPermissionsId };

				await _unitOfWork.BeginTransactionAsync();
				EntityEntry<UserRoles> addRole = Db.UsersRoles.Add(UserRole);
				var addedRows = await Db.SaveChangesAsync();
				if (/*addRole.State is not EntityState.Added ||*/ addedRows <= 0)//TODO: EntityState != Added
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<UserRoles>("Can't add role to user");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<UserRoles>(addRole.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<UserRoles>(e);
			}
		}


		public async Task<ActionResultM<UserM>> IsLoginAvailable(string login)
		{
			try
			{
				UserM? user = await Db.Users
				.AsNoTracking()
				.AsSingleQuery()
				.Include(x => x.Credentials).ThenInclude(x => x.Tokens)
				.Include(x => x.UserProfile)
				.SingleOrDefaultAsync(x => x.UserProfile.Login == login && x.Credentials.AccountVerified == true);

				if (user is null)
				{
					return new ActionResultM<UserM>("User does not exist or User isn't varified");
				}
				else if (user.Credentials.AccountDeleted == true)
				{
					return new ActionResultM<UserM>("Account has been deleted");
				}

				return new ActionResultM<UserM>(user);
			}
			catch (Exception e)
			{
				return new ActionResultM<UserM>(e);
			}
		}


		public async Task<ActionResultM<UserM>> DeleteAsync(UserM entity)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				EntityEntry<UserM> deleteUser = Db.Users.Remove(entity);
				int delRows = await Db.SaveChangesAsync();
				if (deleteUser.State is not EntityState.Deleted || delRows <= 0)
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<UserM>("Nie udało się utworzyć rekordu");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<UserM>(deleteUser.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<UserM>(e);
			}
		}


		public async Task<ActionResultM<UserM>> GetByIdAsync(int id)
		{
			UserM? user = await Db.Users
				 .AsSingleQuery()
				 .AsNoTracking()
				 .Where(x => x.Id == id)
				 .Include(x => x.Credentials).ThenInclude(x => x.Tokens)
				 .Include(x => x.UserProfile)
				 .Select(x => x)
				 .FirstOrDefaultAsync();
			if (user is null)
			{
				return new ActionResultM<UserM>("User does not exist");
			}

			return new ActionResultM<UserM>(user);
		}


		public async Task<ActionResultM<IList<UserM>>> GetListAsync()
		{
			try
			{
				IList<UserM> users = await Db.Users
				.AsNoTracking()
				.AsSingleQuery()
				.ToListAsync();
				if (users is null)
				{
					return new ActionResultM<IList<UserM>>("Something went wrong, collection is empty");
				}

				return new ActionResultM<IList<UserM>>(users);
			}
			catch (Exception e)
			{
				return new ActionResultM<IList<UserM>>(e);
			}
		}


		public async Task<ActionResultM<UserM>> GetUserByTokenAsync(string token)
		{
			try
			{
				UserM? user = await Db.Users
				.AsNoTracking()
				.AsSingleQuery()
				.Include(x => x.Credentials).ThenInclude(x => x.Tokens)
				.Include(x => x.UserProfile)
				.FirstOrDefaultAsync(x => x.Credentials.Tokens.RegistrationJwtToken == token);
				if (user is null)
				{
					return new ActionResultM<UserM>("Token not found or has been used");
				}

				return new ActionResultM<UserM>(user);
			}
			catch (Exception e)
			{
				return new ActionResultM<UserM>(e);
			}
		}


		public async Task<ActionResultM<IAsyncEnumerable<string>>> GetUserRolesAsync(int? userPermissionsId)
		{
			try
			{
				IAsyncEnumerable<string> roles = Db.UsersRoles
				.AsNoTracking()
				.AsSingleQuery()
				.Where(x => x.UserPermissionsId == userPermissionsId)
				.Select(x => x.WebAppRole.WebAppRoleName)
				.AsAsyncEnumerable();
				if (roles is null)
				{
					return new ActionResultM<IAsyncEnumerable<string>>("Something went wrong - getUser haven't goth any roles");
				}
				return new ActionResultM<IAsyncEnumerable<string>>(roles);
			}
			catch (Exception e)
			{
				return new ActionResultM<IAsyncEnumerable<string>>(e);
			}
		}


		public async Task<ActionResultM<UserM>> GetUserByMailAsync(string userEmail)
		{
			try
			{
				UserM? user = await Db.Users
					.AsNoTracking()
					.AsSingleQuery()
					.Include(x => x.Credentials).ThenInclude(x => x.Tokens)
					.Include(x => x.UserProfile)
					.FirstOrDefaultAsync(x => x.UserProfile.Email == userEmail.ToUpper());
				if (user is null)
				{
					return new ActionResultM<UserM>("There's no getUser with given email");
				}

				return new ActionResultM<UserM>(user);
			}
			catch (Exception e)
			{
				return new ActionResultM<UserM>(e);
			}
		}


		public async Task<ActionResultM<Tokens>> SetRefreshTokenNullAsync(string refreshToken)
		{
			try
			{
				Tokens? refreshT = await Db.Tokens
					.AsSingleQuery()
					.AsNoTracking()
					.FirstOrDefaultAsync(x => x.RefreshJwtToken == refreshToken);
				if (refreshT is null)
				{
					return new ActionResultM<Tokens>("You are trying to delete refresh refreshT, but it has already been removed...");
				}

				refreshT.RefreshJwtToken = null;

				await _unitOfWork.BeginTransactionAsync();
				EntityEntry<Tokens> setNewT = Db.Tokens.Update(refreshT);
				int changedRows = await Db.SaveChangesAsync();
				if (/*setNewT.State is not EntityState.Modified || */changedRows <= 0)
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<Tokens>("Can't remove refresh refreshT from data base");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<Tokens>(setNewT.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<Tokens>(e);
			}
		}


		public async Task<ActionResultM<UserM>> MarkAccountDeletedAsync(int userId)
		{
			try
			{
				ActionResultM<UserM> getUser = await GetByIdAsync(userId);
				if (getUser.Status is not ActionStatus.Success)
				{
					return new ActionResultM<UserM>("There is no getUser with given id");
				}

				UserM user = getUser.Data;
				user.Credentials.AccountDeleted = true;

				ActionResultM<UserM> markAcc = await UpdateAsync(user);
				if (markAcc.Status is not ActionStatus.Success)
				{
					return new ActionResultM<UserM>(markAcc.Message);
				}

				return new ActionResultM<UserM>(markAcc.Data);
			}
			catch (Exception)
			{

				throw;
			}
		}


		public async Task<ActionResultM<IAsyncEnumerable<UserM>>> GetDeletedAccountsAsync()
		{
			try
			{
				IAsyncEnumerable<UserM>? accountsToDelete = Db.Users
					.AsNoTracking()
					.AsSingleQuery()
					.Where(x => x.Credentials.AccountDeleted == true).Include(x => x.UserProfile).AsAsyncEnumerable();
				if (accountsToDelete is null)
				{
					return new ActionResultM<IAsyncEnumerable<UserM>>("There are no accounts to delete");
				}

				return new ActionResultM<IAsyncEnumerable<UserM>>(accountsToDelete);
			}
			catch (Exception e)
			{
				return new ActionResultM<IAsyncEnumerable<UserM>>(e);
			}
		}


		public async Task<ActionResultM<UserM>> GetFullUserByIdAsync(int id)
		{
			UserM? user = await Db.Users
				 .Where(x => x.Id == id)
				 .Include(x => x.UserProfile)
				 .Include(x => x.Credentials).ThenInclude(x => x.Tokens)
				 .Include(x => x.Credentials.UserAuthMethod)
				 .Include(x => x.UserPermissions).ThenInclude(x => x.UserRoles).ThenInclude(x => x.WebAppRole)
				 .AsNoTracking()
				 .AsSingleQuery()
				 .Select(x => x)
				 .FirstOrDefaultAsync();
			if (user is null)
			{
				return new ActionResultM<UserM>("User does not exist");
			}

			return new ActionResultM<UserM>(user);
		}


		public async Task<ActionResultM<UserM>> confDelet(int accountToDeleteId)
		{
			try
			{
				ActionResultM<UserM> getUser = await GetFullUserByIdAsync(accountToDeleteId);
				if (getUser.Status is not ActionStatus.Success)
				{
					return new ActionResultM<UserM>("There is no getUser with given id");
				}

				await _unitOfWork.BeginTransactionAsync();
				EntityEntry<UserM> deleteResult = Db.Users.Remove(getUser.Data);
				var delRows = await Db.SaveChangesAsync();
				if (/*deleteResult.State is not EntityState.Deleted || */delRows <= 0)
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<UserM>("Can't delete getUser");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<UserM>(deleteResult.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<UserM>(e);
			}
		}


		public async Task<ActionResultM<UserProfile>> AddUserProfileAsync(UserProfile userProfile)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				Db.Entry(userProfile).State = EntityState.Added;
				EntityEntry<UserProfile> addProfile = Db.UserProfiles.Add(userProfile);
				var addedRows = await Db.SaveChangesAsync();
				if (/*addProfile.State is not EntityState.Added ||*/ addedRows <= 0) //TODO: EntityState == UNCHANGED
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<UserProfile>("Can't add profile");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<UserProfile>(addProfile.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<UserProfile>(e);
			}
		}


		public async Task<ActionResultM<UserProfile>> EmailLoginCheck(SignUpM signUpM)
		{
			try
			{
				IEnumerable<UserProfile>? users = await Db.UserProfiles.ToListAsync();

				UserProfile? serchViaEmail = users.FirstOrDefault(x => x.Email == signUpM.Email.ToUpper());
				UserProfile? serchViaLogin = users.FirstOrDefault(x => x.Login == signUpM.Login);

				if (serchViaEmail is not null)
				{
					return new ActionResultM<UserProfile>("Email is already taken");
				}
				else if (serchViaLogin is not null) 
				{
					return new ActionResultM<UserProfile>("Login is already taken");
				}
			
				return new ActionResultM<UserProfile>("OK", "User can use this credentials");
			}
			catch (Exception e)
			{
				return new ActionResultM<UserProfile>(e);
			}
		}


		public async Task<ActionResultM<UserCredentials>> AddUserCredentialsAsync(UserCredentials userCredentials)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				EntityEntry<UserCredentials> addCred = Db.UserCredentials.Add(userCredentials);
				var addedRows = await Db.SaveChangesAsync();
				if (/*addCred.State is not EntityState.Added || */addedRows <= 0)//TODO: EntityState  = unchanged
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<UserCredentials>("Can't add credentials");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<UserCredentials>(addCred.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<UserCredentials>(e.InnerException);
			}
		}


		public async Task<ActionResultM<UserPermissions>> AddUserPermissionsAsync(UserPermissions userPermissions)
		{
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				EntityEntry<UserPermissions> addPerm = Db.UserPermissions.Add(userPermissions);
				var addedRows = await Db.SaveChangesAsync();
				if (/*addPerm.State is not EntityState.Added ||*/ addedRows <= 0) //TODO: EntityState=unchanged
				{
					await _unitOfWork.RollbackAsync();
					return new ActionResultM<UserPermissions>("Can't add permissions");
				}
				await _unitOfWork.CommitAsync();

				return new ActionResultM<UserPermissions>(addPerm.Entity);
			}
			catch (Exception e)
			{
				await _unitOfWork.RollbackAsync();
				return new ActionResultM<UserPermissions>(e.InnerException);
			}
		}


	}
}
