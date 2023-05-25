using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("UserAuthMethod")]
	public class UserAuthMethod
	{
		[Key]
		public int UserAuthMethodId { get; set; }

		public string? UserAuthMethodName { get; set; }

		public virtual IEnumerable<UserCredentials> UserCredentials { get; set; }

		[NotMapped]
		public static int LDAP = 1;

		[NotMapped]
		public static int Basic = 2;
	}
}
