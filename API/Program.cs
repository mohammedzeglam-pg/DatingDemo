using System;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Hosting;
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
        await context.Database.MigrateAsync();
        await Seed.SeedUsers(context);
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
        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
  }
}
