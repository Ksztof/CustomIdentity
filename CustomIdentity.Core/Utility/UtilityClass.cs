using CustomIdentity.Core.FormModels.IdentityModels;
using CustomIdentity.Core.HelperModels;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CustomIdentity.Core.Utility
{
	public class UtilityClass : IUtilityClass
	{
		private readonly IHttpContextAccessor _httpContextAccessor;


		public UtilityClass(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}



		public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
		{
			using (HMACSHA512 hmac = new HMACSHA512())
			{
				passwordSalt = hmac.Key;
				passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
			}
		}


		public ValidationResult ValidateSignUpForm(SignUpM signUpM)
		{
			//try
			//{
				//string login = signUpM.Login;
				//string firstName = signUpM.FirstName;
				//string lastName = signUpM.LastName;
				//string email = signUpM.Email;
				//string password = signUpM.Password;
				//string passwordCheck = signUpM.PasswordCheck;
				//string phoneNumber = signUpM.PhoneNumber;

				//string loginPattern = @"^[a-zA-Z]{2,}$";
				//string firstAndLastNamePattern = @"^(?=.*[A-Z])[a-zA-Z]{2,}$";
				//string emailPattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
				//string passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{7,}$";
				//string phoneNumberPattern = @"^\+(?:\d{1,3})?(?:\d{6,14})\d$";

				//bool loginPatternIsMatch = Regex.IsMatch(login, loginPattern);
				//bool firstNamePatternIsMatch = Regex.IsMatch(firstName, firstAndLastNamePattern);
				//bool lastNamePatternIsMatch = Regex.IsMatch(lastName, firstAndLastNamePattern);
				//bool emailPatternIsMatch = Regex.IsMatch(email, emailPattern);
				//bool passwordPatternIsMatch = Regex.IsMatch(password, passwordPattern);
				//bool passwordCheckPatternIsMatch = Regex.IsMatch(passwordCheck, passwordPattern);
				//bool phoneNuberPatternIsMatch = Regex.IsMatch(phoneNumber, phoneNumberPattern);

				ValidationResult validationResult = new ValidationResult();

				if (IsInvalidByPattern(signUpM.Login, _NAME_PATTERN))
					validationResult.ErrorMessages.Add("Enter your login again, login should consist of at least two letters");
				//...

				return validationResult;

				//if (string.IsNullOrEmpty(login) || loginPatternIsMatch is false)
				//{
				//	return new ActionResultM<SignUpM>("Enter your login again, login should consist of at least two letters");
				//}
				//else if (string.IsNullOrEmpty(firstName) || firstNamePatternIsMatch is false)
				//{
				//	return new ActionResultM<SignUpM>("Enter your first name again, first name should consist of at least two letters and contain one capital letter");
				//}
				//else if (string.IsNullOrEmpty(lastName) || lastNamePatternIsMatch is false)
				//{
				//	return new ActionResultM<SignUpM>("Enter your last name again, last name should consist of at least two letters and contain one capital letter");
				//}
				//else if (string.IsNullOrEmpty(email) || emailPatternIsMatch is false)
				//{
				//	return new ActionResultM<SignUpM>("Enter your email address again, email should look like: example@example.com");
				//}
				//else if (string.IsNullOrEmpty(password) || passwordPatternIsMatch is false)
				//{
				//	return new ActionResultM<SignUpM>("Enter your password again, password should consist of at least seven characters, one capital letter, one special character and one number");
				//}
				//else if (string.IsNullOrEmpty(passwordCheck) || passwordCheckPatternIsMatch is false || password != passwordCheck)
				//{
				//	return new ActionResultM<SignUpM>("Enter your password check again, password check should be same as your password, consist of at least seven characters, one capital letter, one special character and one number");
				//}
				//else if (string.IsNullOrEmpty(phoneNumber) || phoneNuberPatternIsMatch is false)
				//{
				//	return new ActionResultM<SignUpM>("Enter your phone number including country code, for example +48123123123");
				//}

				//return new ActionResultM<SignUpM>("OK", "Sign up form has been VALIDATED");
			//}
			//catch (Exception e)
			//{
			//	return new ActionResultM<SignUpM>(e);
			//}
		}

		private const string _LOGIN_PATTERN = @"^[a-zA-Z]{2,}$";
		private const string _EMAIL_PATTERN = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
		private const string _NAME_PATTERN = @"^(?=.*[A-Z])[a-zA-Z]{2,}$";
		private const string _PASSWORD_PATTERN = @"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{7,}$";
		private const string _PHONE_NUMBER_PATTERN = @"^\+(?:\d{1,3})?(?:\d{6,14})\d$";

        private bool IsInvalidByPattern(string input, string pattern)
		{
			return string.IsNullOrEmpty(input) || Regex.IsMatch(input, pattern) is false;
        }


		public ActionResultM<SignInM> CheckSignInForm(SignInM signInM)
		{
			try
			{
				string login = signInM.Login;
				string password = signInM.Password;

				string loginPattern = @"^[a-zA-Z]{2,}$";
				string passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{7,}$";

				bool loginPatternIsMatch = Regex.IsMatch(login, loginPattern);
				bool passwordPatternIsMatch = Regex.IsMatch(password, passwordPattern);


				if (string.IsNullOrEmpty(login) || loginPatternIsMatch is false)
				{
					return new ActionResultM<SignInM>("Enter your login again");
				}
				else if (string.IsNullOrEmpty(password) || passwordPatternIsMatch is false)
				{
					return new ActionResultM<SignInM>("Enter your password again");
				}

				return new ActionResultM<SignInM>("OK", "Sign In form has been VALIDATED");
			}
			catch (Exception e)
			{
				return new ActionResultM<SignInM>(e);
			}
		}


		public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
		{
			using (HMACSHA512 hmac = new HMACSHA512(passwordSalt))
			{
				byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
				bool validationResult = computedHash.SequenceEqual(passwordHash);
				return validationResult;
			}
		}



	}
}
