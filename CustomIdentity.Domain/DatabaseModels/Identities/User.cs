using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("User")]
	public class User
	{
		[Key]
		public int Id { get; set; }
		//1:1
		[ForeignKey("UserProfile")]
		public int UserProfileId { get; set; }
		public virtual UserProfile UserProfile { get; set; }

		//1:1
		[ForeignKey("UserCredentials")]
		public int UserCredentialsId { get; set; }
		public virtual UserCredentials Credentials { get; set; }

		//1:1
		[ForeignKey("UserPermissions")]
		public int PermissionsId { get; set; }
		public virtual UserPermissions UserPermissions { get; set; }
	}
}
