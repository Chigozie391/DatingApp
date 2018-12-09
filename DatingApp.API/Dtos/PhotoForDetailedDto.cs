using System;

namespace DatingApp.API.Dtos
{
	public class PhotoForDetailedDto
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public string Description { get; set; }
		public bool isApproved { get; set; }
		public DateTime DateAdded { get; set; }
		public bool isMain { get; set; }

	}
}