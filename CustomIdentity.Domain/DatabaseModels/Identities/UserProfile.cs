using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomIdentity.Domain.DatabaseModels.Identities
{
	[Table("UserProfile")]
	public class UserProfile
	{
		[Key]
		public int UserProfileId { get; set; }

		[ForeignKey("User")]
		public int? UserId { get; set; }
		public virtual User? User { get; set; }

		public string Login { get; set; }
		[MaxLength(50)]
		public string FirstName { get; set; }
		[MaxLength(50)]
		public string LastName { get; set; }
		[EmailAddress]
		public string Email { get; set; }
		public string? Description { get; set; }
		public int? Age { get; set; }
		public int? Score { get; set; }
		public string? UserPhoto { get; set; }
		public string PhoneNumber { get; set; }
		public bool IsOnline { get; set; } = false;
	}
}
