using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace API.Controllers
{

  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public AccountController(DataContext context, ITokenService tokenService)
    {
      _context = context;
      _tokenService = tokenService;
    }


    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register([FromBody] RegisterDto registerDto)
    {
      if (await UserExists(registerDto.Username))
      {
        return BadRequest("User Exists");
      }

      using HMAC hmac = new HMACSHA512();
      AppUser appUser = new()
      {
        UserName = registerDto.Username.ToLower(),
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        PasswordSalt = hmac.Key
      };
      AppUser user = appUser;
      _ = _context.Users.Add(user);
      _ = await _context.SaveChangesAsync();


      return new UserDTO
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDto loginDto)
    {
      AppUser user = await _context.Users.SingleOrDefaultAsync(user => user.UserName == loginDto.Username);
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
          Token = _tokenService.CreateToken(user)
        };
      }

      return Unauthorized("Invalid username");
    }


    private async Task<bool> UserExists(string username)
    {
      return await _context.Users.AnyAsync(user => user.UserName
                                                   == username.ToLower());
    }
  }

}
