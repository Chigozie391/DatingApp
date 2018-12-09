using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
	[Route("api/users/{userid}/photos")]
	public class PhotosController : Controller
	{
		private readonly IMapper _mapper;
		private readonly IDatingRepository _repo;
		//used to retrieve configure options
		private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
		private Cloudinary _cloudinary;

		public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
		{
			_cloudinaryConfig = cloudinaryConfig;
			_repo = repo;
			_mapper = mapper;

			// from cloudinary docs, already defined in the startup class
			Account acc = new Account(
				_cloudinaryConfig.Value.CloudName,
				_cloudinaryConfig.Value.ApiKey,
				_cloudinaryConfig.Value.ApiSecret
			);

			_cloudinary = new Cloudinary(acc);
		}


		// one of the overload for our created route in addPhotoForUser
		[HttpGet("{id}", Name = "GetPhoto")]
		public async Task<IActionResult> GetPhoto(int id)
		{
			var photoFromRepo = await _repo.GetPhoto(id);
			//map
			var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
			return Ok(photo);
		}

		[HttpPost]
		public async Task<IActionResult> AddPhotoForUser(int userid, PhotoForCreationDto photoDto)
		{
			var user = await _repo.GetUser(userid);
			if (user == null)
				return BadRequest("Could not find user");

			// get the current user
			var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

			if (currentUserId != user.Id)
				return Unauthorized();

			var file = photoDto.File;

			// check cloudinary for any confusion
			var uploadResult = new ImageUploadResult();

			if (file.Length > 0)
			{
				// used for reading uploaded file
				using (var stream = file.OpenReadStream())
				{
					var uploadParams = new ImageUploadParams()
					{
						File = new FileDescription(file.Name, stream),
						Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
					};

					//upload file
					uploadResult = _cloudinary.Upload(uploadParams);
				}

				photoDto.Url = uploadResult.Uri.ToString();
				photoDto.PublicId = uploadResult.PublicId;


				//mapping our photo to photodto(PhotoforCreation)
				var photo = _mapper.Map<Photo>(photoDto);
				//assign the user to our photo
				photo.User = user;

				// check if the user does not have a main pic
				if (!user.Photos.Any(m => m.isMain))
					photo.isMain = true;

				//adds the photo
				user.Photos.Add(photo);

				if (await _repo.SaveAll())
				{
					// map it
					var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
					//return some data for the user
					return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
				}

			}

			return BadRequest("Could not add the photo");
		}

		[HttpPost("{id}/setMain")]
		public async Task<IActionResult> SetMainPhoto(int userId, int id)
		{
			if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			var photoFromRepo = await _repo.GetPhoto(id);
			if (photoFromRepo == null)
				return NotFound();

			if (photoFromRepo.isMain)
				return BadRequest("This is already the main photo");

			var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
			if (currentMainPhoto != null)
				currentMainPhoto.isMain = false;

			photoFromRepo.isMain = true;

			if (await _repo.SaveAll())
				return NoContent();

			return BadRequest("Could not set photo to main");

		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePhoto(int userId, int id)
		{
			if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			var photoFromRepo = await _repo.GetPhoto(id);
			if (photoFromRepo == null)
				return NotFound();

			if (photoFromRepo.isMain)
				return BadRequest("You can not delete the main photo");

			if (photoFromRepo.PublicId != null)
			{
				var deleteParams = new DeletionParams(photoFromRepo.PublicId);
				var result = _cloudinary.Destroy(deleteParams);

				if (result.Result == "ok")
					_repo.Delete(photoFromRepo);

			}

			if (photoFromRepo.PublicId == null)
			{
				_repo.Delete(photoFromRepo);
			}

			if (await _repo.SaveAll())
				return Ok();

			return BadRequest("Failed to delete the Photo");
		}

	}
}