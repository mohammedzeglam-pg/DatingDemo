using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
namespace API.Helpers
{
  public class AutoMappperProfiles : Profile
  {
    public AutoMappperProfiles()
    {
      _ = CreateMap<AppUser, MemberDto>()
        .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(photos => photos.IsMain).Url))
        .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
      _ = CreateMap<Photo, PhotoDto>();
      _ = CreateMap<MemberUpdateDto, AppUser>();
      _ = CreateMap<RegisterDto, AppUser>();
      _ = CreateMap<AppUser, LikeDto>()
        .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(photos => photos.IsMain).Url))
        .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
      _ = CreateMap<Message, MessageDto>()
        .ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(photos => photos.IsMain).Url))
        .ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(photos => photos.IsMain).Url));
    }
  }
}
