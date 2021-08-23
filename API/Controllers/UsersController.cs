using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Extensions;
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
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
      return base.Ok(await _userRepository.GetMembersAsync());
    }


    [HttpGet("{username}", Name = "GetUser")]
    public async Task<ActionResult<MemberDto>> GetUserAsync(string username)
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
