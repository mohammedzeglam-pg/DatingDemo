using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class Seed
  {
    public static async Task SeedUsers(DataContext context)
    {
      if (await context.Users.AnyAsync()) return;
      byte[] userData = await System.IO.File.ReadAllBytesAsync("Data/UserSeedData.json");
      List<AppUser> users = JsonSerializer.Deserialize<List<AppUser>>(userData);
      foreach (AppUser user in users)
      {
        using HMACSHA512 hmac = new();
        user.UserName = user.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password"));
        user.PasswordSalt = hmac.Key;
        _ = context.Users.Add(user);
      }
      _ = await context.SaveChangesAsync();
    }
  }
}
