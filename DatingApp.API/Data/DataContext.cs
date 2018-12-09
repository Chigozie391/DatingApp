using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{

	// adding configurations to get the user with the roles and also make the id an integer
	public class DataContext : IdentityDbContext<User, Role, int,
		IdentityUserClaim<int>, UserRole,
		IdentityUserLogin<int>, IdentityRoleClaim<int>,
		IdentityUserToken<int>>
	{
		public DataContext(DbContextOptions<DataContext> options) : base(options) { }
		public DbSet<Value> Values { get; set; }
		public DbSet<User> User { get; set; }
		public DbSet<Photo> Photos { get; set; }
		public DbSet<Like> Likes { get; set; }
		public DbSet<Message> Messages { get; set; }

		//many to many relationship

		protected override void OnModelCreating(ModelBuilder builder)
		{
			//we are usinf the identityDb , we need to call the base and 
			// passin the builder
			base.OnModelCreating(builder);

			//configure primary key
			builder.Entity<UserRole>(userRole =>
			{
				userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

				//one to many relationship
				//role has many user
				userRole.HasOne(ur => ur.Role)
				.WithMany(r => r.UserRoles)
				.HasForeignKey(ur => ur.RoleId)
				.IsRequired();

				//one to many relationship
				// user has many roles
				userRole.HasOne(ur => ur.User)
				.WithMany(u => u.UserRoles)
				.HasForeignKey(ur => ur.UserId)
				.IsRequired();
			});


			//using likee and liker to make up the primary key
			builder.Entity<Like>()
				.HasKey(k => new { k.LikerId, k.LikeeId });

			//configuring double one to many relationship
			builder.Entity<Like>()
				.HasOne(u => u.Likee)
				.WithMany(u => u.Liker)
				.HasForeignKey(u => u.LikeeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Like>()
				.HasOne(u => u.Liker)
				.WithMany(u => u.Likee)
				.HasForeignKey(u => u.LikerId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Message>()
				.HasOne(u => u.Sender)
				.WithMany(m => m.MessageSent)
				.OnDelete(DeleteBehavior.Restrict);


			builder.Entity<Message>()
				.HasOne(u => u.Recipient)
				.WithMany(m => m.MessageRecieve)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Photo>().HasQueryFilter(p => p.isApproved);

		}


	}


}