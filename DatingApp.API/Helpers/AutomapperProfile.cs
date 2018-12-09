using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
	public class AutomapperProfile : Profile
	{
		//we need to tell automapper themap we are going to create
		public AutomapperProfile()
		{
			//we stil nedd to instruct the mapper on fields like photourl that doesnt corresspond with the user class
			//we will map the main photo(url) of the user to the photourl if its the main one

			CreateMap<User, UserForDetailedDto>()
			.ForMember(des => des.PhotoUrl, opt =>
			{
				// source is photos and it it checks if its the main photo, if its true return the url
				opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.isMain).Url);
			}).ForMember(dest => dest.Age, opt =>
			{
				//we use resolveusing because we are not remapping from another field
				opt.ResolveUsing(d => d.DateOfBirth.CalculateAge());
			});



			CreateMap<User, UserForListDto>()
			.ForMember(des => des.PhotoUrl, opt =>
			{
				//gets the main photo
				// source is photos and it it checks if its the main photo, if its true return the url
				opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.isMain).Url);
			}).ForMember(dest => dest.Age, opt =>
			{
				//we use resolveusing because we are not remapping from another field
				opt.ResolveUsing(d => d.DateOfBirth.CalculateAge());
			});

			CreateMap<Photo, PhotoForDetailedDto>();
			//map for updating the user
			CreateMap<UserForUpdateDto, User>();
			//for photos
			CreateMap<PhotoForCreationDto, Photo>();
			CreateMap<Photo, PhotoForReturnDto>();
			CreateMap<UserForRegisterDto, User>();

			CreateMap<MessageForCreationDto, Message>();

			//ampper was able to detect most fiels excepts for sender and reciever ur;
			CreateMap<Message, MessageToReturnDto>()
				.ForMember(m => m.SenderPhotoUrl, opt =>
					 opt.MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.isMain).Url))
				.ForMember(m => m.RecipientPhotoUrl, opt =>
					 opt.MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.isMain).Url));

		}

	}
}