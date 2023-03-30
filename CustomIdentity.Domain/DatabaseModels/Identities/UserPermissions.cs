using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("UserPermissions")]
	public class UserPermissions
	{
		[Key]
		public int UserPermissionsId { get; set; }

		[ForeignKey("User")]
		public int? UserId { get; set; }
		public virtual User? User { get; set; }
		public virtual IEnumerable<UserRoles>? UserRoles { get; set; }
	}
}
