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

	[ServiceFilter(typeof(LogUserActvity))]
	[Route("api/users/{userid}/[controller]")]
	[ApiController]
	public class MessagesController : ControllerBase
	{
		private readonly IDatingRepository _repo;
		private readonly IMapper _mapper;
		public MessagesController(IDatingRepository repo, IMapper mapper)
		{
			_mapper = mapper;
			_repo = repo;
		}

		[HttpGet("{id}", Name = "GetMessage")]
		public async Task<IActionResult> GetMessage(int userid, int id)
		{
			if (userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			var messageFromRepo = await _repo.GetMessage(id);
			if (messageFromRepo == null)
				return NotFound();

			return Ok(messageFromRepo);
		}
		[HttpGet]
		public async Task<IActionResult> GetMessagesForUser(int userid, MessageParams messageParams)
		{
			if (userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			var messageFromRepo = await _repo.GetMessagesForUser(messageParams);
			var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);

			Response.AddPagination(messageFromRepo.CurrentPage, messageFromRepo.PageSize,
					messageFromRepo.TotalCount, messageFromRepo.TotalPages);

			return Ok(messages);
		}
		[HttpGet("thread/{id}")]
		public async Task<IActionResult> GetMessageThread(int userId, int id)
		{
			if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			var messageFromRepo = await _repo.GetMessagesThread(userId, id);

			var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);

			return Ok(messageThread);
		}


		[HttpPost]
		public async Task<IActionResult> CreateMessage(int userid, [FromBody] MessageForCreationDto messageForCreationDto)
		{
			if (userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			messageForCreationDto.SenderId = userid;

			var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);
			var sender = await _repo.GetUser(messageForCreationDto.SenderId);

			if (recipient == null)
				return BadRequest("Could not find user");

			// even though we are not adding the recipient or sender information, automapper adds it
			// to the map because its in he memory
			var message = _mapper.Map<Message>(messageForCreationDto);

			_repo.Add(message);

			var messageToReturn = _mapper.Map<MessageToReturnDto>(message);

			if (await _repo.SaveAll())
				return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);

			throw new Exception("Creating the message failed on save");
		}

		[HttpPost("{id}")]
		public async Task<IActionResult> DeleteMessage(int id, int userid)
		{
			if (userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			var messageFromRepo = await _repo.GetMessage(id);

			if (messageFromRepo.SenderId == userid)
				messageFromRepo.SenderDelete = true;

			if (messageFromRepo.RecipientId == userid)
				messageFromRepo.RecipientDeleted = true;

			if (messageFromRepo.SenderDelete && messageFromRepo.RecipientDeleted)
				_repo.Delete(messageFromRepo);

			if (await _repo.SaveAll())
				return NoContent();


			throw new Exception("Error Deleting the message");
		}

		[HttpPost("{id}/read")]
		public async Task<IActionResult> MarkMessageAsRead(int userid, int id)
		{
			if (userid != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
				return Unauthorized();

			var message = await _repo.GetMessage(id);

			if (message.RecipientId != userid)
				return BadRequest("Failed to mark message as Read");

			message.isRead = true;
			message.DateRead = DateTime.Now;

			await _repo.SaveAll();
			return NoContent();
		}

	}


}