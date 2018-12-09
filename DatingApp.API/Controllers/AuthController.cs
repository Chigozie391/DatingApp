using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
	[AllowAnonymous]
	[Route("api/[controller]")]
	public class AuthController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IConfiguration _config;
		private readonly IMapper _mapper;

		public AuthController(UserManager<User> userManager, SignInManager<User> signInManager,
		 IConfiguration config, IMapper mapper)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_config = config;
			_mapper = mapper;
		}


		[HttpPost("register")]
		//pick the username and password from the body
		public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegisterDto)
		{
			//create the username for the user registeration
			var userToCreate = _mapper.Map<User>(userForRegisterDto);

			var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

			var user = _mapper.Map<UserForDetailedDto>(userToCreate);

			if (result.Succeeded)
			{
				return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, user);
			}

			return BadRequest(result.Errors);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody]UserForLoginDto userForLoginDto)
		{

			var userLogin = await _userManager.FindByNameAsync(userForLoginDto.Username);
			var result = await _signInManager.CheckPasswordSignInAsync(userLogin, userForLoginDto.Password, false);

			if (result.Succeeded)
			{
				var appuser = await _userManager.Users.Include(p => p.Photos)
				.FirstOrDefaultAsync(u => u.NormalizedUserName == userForLoginDto.Username.ToUpper());

				var user = _mapper.Map<UserForListDto>(appuser);

				return Ok(new { tokenString = GenerateJwtToken(appuser).Result, user });
			}

			return Unauthorized();
		}

		private async Task<string> GenerateJwtToken(User user)
		{

			var claim = new List<Claim>
			{
				//id as the identitofer
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				//username as  the name
				new Claim(ClaimTypes.Name,user.UserName),
			};

			//get the user roles
			var roles = await _userManager.GetRolesAsync(user);

			foreach (var role in roles)
			{
				claim.Add(new Claim(ClaimTypes.Role, role));
			}


			var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:Token").Value));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claim),
				Expires = DateTime.Now.AddDays(1),
				SigningCredentials = creds
			};
			//generate token
			var tokenHandler = new JwtSecurityTokenHandler();

			var token = tokenHandler.CreateToken(tokenDescriptor);
			var tokenString = tokenHandler.WriteToken(token);
			return tokenString;
		}

	}
}