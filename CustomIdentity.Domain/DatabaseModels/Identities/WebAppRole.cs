using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("WebAppRole")]
	public class WebAppRole
	{
		[Key]
		public int WebAppRoleId { get; set; }
		public string WebAppRoleName { get; set; }
		public virtual IEnumerable<UserRoles> UserRoles { get; set; }
	}
}
