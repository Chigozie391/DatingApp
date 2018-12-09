using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Models
{
	public class User : IdentityUser<int>
	{
		public string Gender { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string KnownAs { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastActive { get; set; }
		public string Introduction { get; set; }
		public string LookingFor { get; set; }
		public string Interests { get; set; }
		public string City { get; set; }
		public string Country { get; set; }
		// storing one to many rrelationship of photo
		public ICollection<Photo> Photos { get; set; }
		// initialize the collection of Photos in the constructor
		public User()
		{
			Photos = new Collection<Photo>();
		}

		//starting many to many relationship using fluent API

		//each user can have many liker
		public ICollection<Like> Liker { get; set; }

		//each user can have many likee
		public ICollection<Like> Likee { get; set; }

		public ICollection<Message> MessageSent { get; set; }
		public ICollection<Message> MessageRecieve { get; set; }

		// naviagtion property for userRole
		public ICollection<UserRole> UserRoles { get; set; }

	}
}