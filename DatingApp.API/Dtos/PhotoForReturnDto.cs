using System;

namespace DatingApp.API.Dtos
{
	public class PhotoForReturnDto
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public string Description { get; set; }
		public DateTime DateAdded { get; set; }
		// public bool isApproved { get; set; }
		public bool isMain { get; set; }
		//for cloudinary
		public string PublicId { get; set; }
	}
}