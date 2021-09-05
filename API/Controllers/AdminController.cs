using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AdminController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
      _userManager = userManager;
    }

    [Authorize(Policy = "RequiredAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
      var users = await _userManager.Users
              .Include(u => u.UserRoles)
              .ThenInclude(u => u.Role)
              .OrderBy(u => u.UserName)
              .Select(u => new
              {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList(),
              })
              .ToListAsync();
      return Ok(users);
    }

    [Authorize(Policy = "RequiredAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRole(string username, [FromQuery] string roles)
    {
      string[] selectedRoles = roles.Split(',').ToArray();
      AppUser user = await _userManager.FindByNameAsync(username);
      if (user == null)
      {
        return NotFound("Could not found user");
      }

      IList<string> userRoles = await _userManager.GetRolesAsync(user);
      IdentityResult result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
      if (!result.Succeeded)
      {
        return BadRequest("Failed to add to roles");
      }

      result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(userRoles));
      return !result.Succeeded ? BadRequest("Failed to remove from roles") : Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
      return Ok("Admins or photos moderation can see that");
    }
  }
}
