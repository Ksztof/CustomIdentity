using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("UserCredentials")]
	public class UserCredentials
	{
		[Key]
		public int UserCredentialsId { get; set; }

		[ForeignKey("User")]
		public int? UserId { get; set; }
		public virtual User? User { get; set; }
		public string CredentialName { get; set; }
		public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }
		public bool AccountVerified { get; set; } = false;
		public bool AccountDeleted { get; set; } = false;
		public virtual Tokens? Tokens { get; set; }
		public int? UserAuthMethodId { get; set; }
		public virtual UserAuthMethod? UserAuthMethod { get; set; }
	}
}
