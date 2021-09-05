using System;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      /* CreateHostBuilder(args).Build().Run(); */
      IHost host = CreateHostBuilder(args).Build();
      using IServiceScope scope = host.Services.CreateScope();
      IServiceProvider services = scope.ServiceProvider;
      try
      {
        DataContext context = services.GetRequiredService<DataContext>();
        UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();
        RoleManager<AppRole> roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        await Task.WhenAll(context.Database.MigrateAsync(), Seed.SeedUsers(userManager, roleManager));
      }
      catch (Exception ex)
      {
        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Problem in migration");
      }
      await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      return Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
            {
              _ = logging.ClearProviders();
              _ = logging.AddConsole();
              _ = logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
              _ = logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
            })
        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
  }
}
