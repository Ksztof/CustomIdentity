using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("Tokens")]
	public class Tokens
	{
		[Key]
		public int TokensId { get; set; }

		public string? RegistrationJwtToken { get; set; }

		public string? RefreshJwtToken { get; set; }

		[ForeignKey("UserCredential")]
		public int? UserCredentialsId { get; set; }

		public UserCredentials? UserCredential { get; set; }
	}


}
