using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class Seed
  {
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
      if (await userManager.Users.AnyAsync())
      {
        return;
      }

      byte[] userData = await System.IO.File.ReadAllBytesAsync("Data/UserSeedData.json");
      List<AppUser> users = JsonSerializer.Deserialize<List<AppUser>>(userData);
      if (users == null)
      {
        return;
      }
      List<AppRole> roles = new()
      {
        new() { Name = "Member" },
        new() { Name = "Admin" },
        new() { Name = "Moderator" },
      };
      foreach (AppRole role in roles)
      {
        _ = await roleManager.CreateAsync(role);
      }

      foreach (AppUser user in users)
      {
        user.UserName = user.UserName.ToLower(CultureInfo.CurrentCulture);
        _ = await userManager.CreateAsync(user, "Pa$$w0rd");
        _ = await userManager.AddToRoleAsync(user, "Member");
      }
      AppUser admin = new()
      {
        UserName = "admin",

      };
      _ = await userManager.CreateAsync(admin, "Pa$$w0rd");
      _ = await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
    }
  }
}
