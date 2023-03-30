using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomIdentity.Core.FormModels.IdentityModels
{
	public class ChooseAccountToDeleteM
	{
		[Required]
		public int AccountId { get; set; }
	}
}
