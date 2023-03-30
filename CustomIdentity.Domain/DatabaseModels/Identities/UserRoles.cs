using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("UserRoles")]
	public class UserRoles
	{
		public int? UserPermissionsId { get; set; }
		public virtual UserPermissions? UserPermissions { get; set; }
		public int? WebAppRoleId { get; set; }
		public virtual WebAppRole? WebAppRole { get; set; }
	}

	public enum RolesValue
	{
		USER = 1,
		ADMIN = 2,
	}

}
