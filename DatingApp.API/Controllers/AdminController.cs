using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AdminController : ControllerBase
	{
		private readonly DataContext _context;
		private readonly UserManager<User> _userManager;
		private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
		private Cloudinary _cloudinary;

		public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloudinaryConfig)
		{
			_context = context;
			_userManager = userManager;
			_cloudinaryConfig = cloudinaryConfig;

			Account acc = new Account(_cloudinaryConfig.Value.CloudName, _cloudinaryConfig.Value.ApiKey, _cloudinaryConfig.Value.ApiSecret);
			_cloudinary = new Cloudinary(acc);
		}

		[Authorize(Policy = "RequireAdminRole")]
		[HttpGet("userWithRoles")]
		public async Task<IActionResult> GetUserWithRoles()
		{
			// get the user along with his roles
			// check vid 1 for his method(expression method)
			var userList = await _context.Users.OrderBy(u => u.UserName).Select(u => new
			{
				Id = u.Id,
				UserName = u.UserName,
				Roles = u.UserRoles.Join(_context.Roles, ur => ur.RoleId, r => r.Id, (userRole, roles) => roles.Name)
			}).ToListAsync();

			return Ok(userList);
		}

		[Authorize(Policy = "RequireAdminRole")]
		[HttpPost("editRoles/{username}")]
		public async Task<IActionResult> EditRoles(string username, [FromBody] RoleEditDto roleEditDto)
		{
			var user = await _userManager.FindByNameAsync(username);
			//get roles
			var userRoles = await _userManager.GetRolesAsync(user);

			var selectedRole = roleEditDto.RoleNames;

			// selectedRole != null ? selectedROles : new string[] {};
			selectedRole = selectedRole ?? new string[] { };

			//add the roles to the user eexcept the one that already exist
			var result = await _userManager.AddToRolesAsync(user, selectedRole.Except(userRoles));

			if (!result.Succeeded)
				return BadRequest("Failed to add to roles");

			//remove the ones that has been removed
			result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRole));

			if (!result.Succeeded)
				return BadRequest("Failed to remove to roles");

			return Ok(await _userManager.GetRolesAsync(user));
		}

		[Authorize(Policy = "ModeratorPhotoRole")]
		[HttpGet("photoForModeration")]
		public async Task<IActionResult> GetPhotosForModeration()
		{
			var photo = await _context.Photos
				.Include(u => u.User)
				.IgnoreQueryFilters()
				.Where(p => p.isApproved == false)
				.Select(x => new
				{
					id = x.Id,
					UserName = x.User.UserName,
					Url = x.Url,
					isApproved = x.isApproved
				}).ToListAsync();

			return Ok(photo);
		}


		[Authorize(Policy = "ModeratorPhotoRole")]
		[HttpPost("approvePhoto/{photoId}")]
		public async Task<IActionResult> ApprovePhoto(int photoId)
		{
			var photo = await _context.Photos.IgnoreQueryFilters()
				.FirstOrDefaultAsync(p => p.Id == photoId);

			photo.isApproved = true;
			await _context.SaveChangesAsync();

			return Ok();
		}


		[Authorize(Policy = "ModeratorPhotoRole")]
		[HttpDelete("rejectPhoto/{photoId}")]
		public async Task<IActionResult> RejectPhoto(int photoId)
		{
			var photo = await _context.Photos.IgnoreQueryFilters()
				.FirstOrDefaultAsync(p => p.Id == photoId);

			if (photo == null)
				return NotFound();

			if (photo.isMain)
				return BadRequest("You can not reject the main photo");

			if (photo.PublicId != null)
			{
				var deleteParams = new DeletionParams(photo.PublicId);
				var result = _cloudinary.Destroy(deleteParams);

				if (result.Result == "ok")
					_context.Remove(photo);

			}

			if (photo.PublicId == null)
			{
				_context.Remove(photo);
			}

			await _context.SaveChangesAsync();

			return Ok();
		}

	}

}