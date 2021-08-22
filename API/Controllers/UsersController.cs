using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

  [Authorize]
  public class UsersController : BaseApiController
  {
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;


    public UsersController(IUserRepository userRepository,
      IMapper mapper)
    {
      _userRepository = userRepository;
      _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
      return base.Ok(await _userRepository.GetMembersAsync());
    }


    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUserAsync(string username)
    {
      return await _userRepository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto member)
    {
      string username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      AppUser user = await _userRepository.GetUserByUsernameAsync(username);
      _ = _mapper.Map(member, user);
      _userRepository.UpdateUser(user);
      return await _userRepository.SaveAllChangesAsync() ? NoContent() : BadRequest("failed to update user");
    }

  }
}
