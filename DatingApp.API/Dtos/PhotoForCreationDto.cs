using System;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Dtos
{
	public class PhotoForCreationDto
	{
		public string Url { get; set; }
		// for messing with files
		public IFormFile File { get; set; }
		public string Description { get; set; }
		public DateTime DateAdded { get; set; }
		public string PublicId { get; set; }
		//initialize the dateAdded
		public PhotoForCreationDto()
		{
			DateAdded = DateTime.Now;
		}


	}
}