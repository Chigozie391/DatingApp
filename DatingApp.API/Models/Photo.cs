using System;

namespace DatingApp.API.Models
{
	public class Photo
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public string Description { get; set; }
		public DateTime DateAdded { get; set; }
		public bool isMain { get; set; }
		//for cloudinary
		public string PublicId { get; set; }
		public bool isApproved { get; set; }

		//to get cascade effect when we delete a user, we also delte the photos
		public User User { get; set; }
		public int UserId { get; set; }

	}
}