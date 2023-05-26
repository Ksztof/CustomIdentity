using CustomIdentity.Core.FormModels.IdentityModels;
using CustomIdentity.Core.HelperModels;

namespace CustomIdentity.Core.Utility
{
	public interface IUtilityClass
	{
		public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
		public ValidationResult ValidateSignUpForm(SignUpM signUpM);
		public ActionResultM<SignInM> CheckSignInForm(SignInM signUpM);
		public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
	}
}
