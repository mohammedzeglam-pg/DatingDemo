using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    private readonly IMapper _mapper;

    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
      _context = context;
      _tokenService = tokenService;
      _mapper = mapper;
    }


    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register([FromBody] RegisterDto registerDto)
    {
      if (await UserExists(registerDto.Username))
      {
        return BadRequest("User Exists");
      }

      AppUser user = _mapper.Map<AppUser>(registerDto);
      using HMAC hmac = new HMACSHA512();
      user.UserName = registerDto.Username.ToLower(CultureInfo.CurrentCulture);
      user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
      user.PasswordSalt = hmac.Key;

      _ = _context.Users.Add(user);
      _ = await _context.SaveChangesAsync();


      return new UserDTO
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user),
        KnownAs = user.KnownAs,

        Gender = user.Gender
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDto loginDto)
    {
      AppUser user = await _context.Users
        .Include(user => user.Photos)
        .SingleOrDefaultAsync(user => user.UserName == loginDto.Username);
      if (user != null)
      {
        using HMAC hmac = new HMACSHA512(user.PasswordSalt);
        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        for (int i = 0; i < computedHash.Length; i++)
        {
          if (computedHash[i] == user.PasswordHash[i])
          {
            continue;
          }

          return Unauthorized("Invalid password");
        }

        return new UserDTO
        {
          Username = user.UserName,
          Token = _tokenService.CreateToken(user),
          PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
          KnownAs = user.KnownAs,
          Gender = user.Gender
        };
      }

      return Unauthorized("Invalid username");
    }


    private async Task<bool> UserExists(string username)
    {
      return await _context.Users.AnyAsync(user => user.UserName == username.ToLower(CultureInfo.CurrentCulture));
    }
  }

}
