using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("User")]
	public class User
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("UserProfile")]
		public int UserProfileId { get; set; }

		public virtual UserProfile UserProfile { get; set; }

		[ForeignKey("UserCredentials")]
		public int UserCredentialsId { get; set; }

		public virtual UserCredentials Credentials { get; set; }

		[ForeignKey("UserPermissions")]
		public int PermissionsId { get; set; }

		public virtual UserPermissions UserPermissions { get; set; }
	}
}
