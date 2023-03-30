using CustomIdentity.Domain.DatabaseModels.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Xml.Linq;

namespace CustomIdentity.Domain
{
	public class CustomIdentityDb : DbContext
	{
		private readonly IConfiguration _configuration;

		public virtual DbSet<User> Users { get; set; }
		public virtual DbSet<UserAuthMethod> UserAuthMethods { get; set; }
		public virtual DbSet<UserCredentials> UserCredentials { get; set; }
		public virtual DbSet<UserRoles> UsersRoles { get; set; }
		public virtual DbSet<WebAppRole> WebAppRoles { get; set; }
		public virtual DbSet<Tokens> Tokens { get; set; }
		public virtual DbSet<UserPermissions> UserPermissions { get; set; }
		public virtual DbSet<UserProfile> UserProfiles { get; set; }

		public CustomIdentityDb()
		{
		}

		public CustomIdentityDb(DbContextOptions<CustomIdentityDb> options) : base(options)
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseNpgsql("User ID =postgres;Password=\"haslo1234\";Server=localhost;Port=5432;Database=CustomIdentityDb;");
			}
		}


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			//User-UserProfile 1-1
			modelBuilder.Entity<User>()
				.HasOne(u => u.UserProfile)
				.WithOne(up => up.User)
				.HasForeignKey<UserProfile>(up => up.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserProfile>()
				.HasOne(up => up.User)
				.WithOne(u => u.UserProfile)
				.HasForeignKey<UserProfile>(up => up.UserId);

			//User-UserCredentials 1-1
			modelBuilder.Entity<User>()
				.HasOne(u => u.Credentials)
				.WithOne(c => c.User)
				.HasForeignKey<UserCredentials>(c => c.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserCredentials>()
				.HasOne(c => c.User)
				.WithOne(u => u.Credentials)
				.HasForeignKey<UserCredentials>(c => c.UserId);

			//User-UserPermissions 1-1
			modelBuilder.Entity<User>()
				.HasOne(u => u.UserPermissions)
				.WithOne(up => up.User)
				.HasForeignKey<UserPermissions>(up => up.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserPermissions>()
				.HasOne(up => up.User)
				.WithOne(u => u.UserPermissions)
				.HasForeignKey<UserPermissions>(up => up.UserId);

			//UserCredentials - UserAuthMethod 1-*
			modelBuilder.Entity<UserCredentials>()
				.HasOne(x => x.UserAuthMethod)
				.WithMany(y => y.UserCredentials)
				.HasForeignKey(x => x.UserAuthMethodId)
				.OnDelete(DeleteBehavior.Cascade);

			//UserPermissions-UserRoles-WebAppRole *-*
			modelBuilder.Entity<UserRoles>().HasKey(sc => new { sc.UserPermissionsId, sc.WebAppRoleId });

			modelBuilder.Entity<UserRoles>()
				.HasOne(sc => sc.UserPermissions)
				.WithMany(s => s.UserRoles)
				.HasForeignKey(sc => sc.UserPermissionsId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<UserRoles>()
				.HasOne(sc => sc.WebAppRole)
				.WithMany(s => s.UserRoles)
				.HasForeignKey(sc => sc.WebAppRoleId)
				.OnDelete(DeleteBehavior.Cascade);

			SeedBaseData(modelBuilder);
		}

		private void SeedBaseData(ModelBuilder modelBuilder)
		{
		}


	}
}

