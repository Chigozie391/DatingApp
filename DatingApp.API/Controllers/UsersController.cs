using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
	//udates the lastActive
	[ServiceFilter(typeof(LogUserActvity))]
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IDatingRepository _repo;
		private readonly IMapper _mapper;

		public UsersController(IDatingRepository repo, IMapper mapper)
		{
			_repo = repo;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> GetUsers(UserParams userParams)
		{
			//UserParams are coming from queery, we don really nedd to specify that

			var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			//get user from database
			var userFromRepo = await _repo.GetUser(currentUserId);

			userParams.UserId = currentUserId;

			// set the genders to get from repo if not from query
			if (string.IsNullOrEmpty(userParams.Gender))
			{
				userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
			}

			var users = await _repo.GetUsers(userParams);
			var userToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

			Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
			return Ok(userToReturn);
		}

		[HttpGet("{id}", Name = "GetUser")]
		public async Task<IActionResult> GetUser(int id)
		{
			var isCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) == id;

			var user = await _repo.GetUser(id, isCurrentUser);
			var userToReturn = _mapper.Map<UserForDetailedDto>(user);
			return Ok(userToReturn);
		}

		// api/users/1 PUT:
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser(int id, [FromBody] UserForUpdateDto userForUpdateDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			// get the user id using claims
			var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			//get user from database
			var userFromRepo = await _repo.GetUser(id);

			if (userFromRepo == null)
				return NotFound($"Could not find user with an ID od {id}");

			if (currentUserId != userFromRepo.Id)
				return Unauthorized();

			// map our details
			_mapper.Map(userForUpdateDto, userFromRepo);

			if (await _repo.SaveAll())
				return NoContent();

			throw new Exception($"Updating user Id Failed on Save");
		}

		[HttpPost("{id}/like/{recipientId}")]
		public async Task<IActionResult> LikeUser(int id, int recipientId)
		{
			if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			var like = await _repo.GetLike(id, recipientId);

			if (like != null)
				return BadRequest("You already liked this user");

			//check if the recipient id exist
			if (await _repo.GetUser(recipientId) == null)
				return NotFound();

			like = new Like
			{
				LikerId = id,
				LikeeId = recipientId
			};

			_repo.Add<Like>(like);

			if (await _repo.SaveAll())
				return Ok(new { });

			return BadRequest("Failed to like user");
		}

	}
}