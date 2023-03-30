using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomIdentity.Core.FormModels.IdentityModels
{
	public class AccountsToDeleteM
	{
		[Required]
		public int UserId { get; set; }
		[Required]
		public int? UserProfileId { get; set; }
		[Required]
		public string? UserName { get; set; }
		[Required]
		public string? UserEmail { get; set; }
	}
}
