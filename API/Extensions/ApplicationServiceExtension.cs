using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SingalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
  public static class ApplicationServiceExtension
  {
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config
    )
    {

      _ = services.AddSingleton<PresenceTracker>();
      _ = services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
      _ = services.AddScoped<ITokenService, TokenService>();
      _ = services.AddScoped<IUserRepository, UserRepository>();
      _ = services.AddScoped<IMessageRepository, MessageRepository>();
      _ = services.AddScoped<ILikesRepository, LikesRepository>();
      _ = services.AddScoped<IPhotoService, PhotoService>();
      _ = services.AddScoped<LogUserActivity>();
      _ = services.AddAutoMapper(typeof(AutoMappperProfiles).Assembly);
      _ = services.AddDbContext<DataContext>(options =>
            {
              _ = options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
      return services;

    }
  }
}
