using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

  [Authorize]
  public class UsersController : BaseApiController
  {
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;




    public UsersController(IUserRepository userRepository,
      IMapper mapper,
      IPhotoService photoService
      )
    {
      _userRepository = userRepository;
      _mapper = mapper;
      _photoService = photoService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
      AppUser user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
      if (string.IsNullOrEmpty(userParams.Gender))
      {
        userParams.Gender = user.Gender == "male" ? "female" : "male";
      }
      PagedList<MemberDto> users = await _userRepository.GetMembersAsync(userParams);
      Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
      return Ok(users);
    }



    [HttpGet("{username}", Name = "GetUser")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
      return await _userRepository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto member)
    {
      AppUser user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
      _ = _mapper.Map(member, user);
      _userRepository.UpdateUser(user);
      return await _userRepository.SaveAllChangesAsync() ? NoContent() : BadRequest("failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {

      AppUser user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
      ImageUploadResult result = await _photoService.AddPhotoAsync(file);
      if (result.Error != null)
      {
        return BadRequest(result.Error.Message);
      }
      Photo photo = new()
      {
        Url = result.SecureUrl.AbsoluteUri,
        PublicId = result.PublicId
      };
      if (user.Photos.Count == 0)
      {
        photo.IsMain = true;
      }
      user.Photos.Add(photo);
      return await _userRepository.SaveAllChangesAsync()
          ? CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo))
          : BadRequest("Problem when add x");
    }
    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
      AppUser user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
      Photo photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
      if (photo.IsMain)
      {
        return BadRequest("already main phtot");
      }
      Photo currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
      if (currentMain != null)
      {
        currentMain.IsMain = false;
      }
      photo.IsMain = true;
      return await _userRepository.SaveAllChangesAsync() ? NoContent() : BadRequest("failed to set main photo");
    }
    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhotot(int photoId)
    {
      AppUser user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
      Photo photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
      if (photo == null)
      {
        return NotFound();
      }
      if (photo.PublicId != null)
      {
        DeletionResult result = await _photoService.DeletePhotoAsync(photo.PublicId);
        if (result.Error == null)
        {
          return BadRequest(result.Error.Message);
        }
      }
      _ = user.Photos.Remove(photo);
      return await _userRepository.SaveAllChangesAsync() ? Ok() : BadRequest("Failed to delete photo");
    }
  }
}
