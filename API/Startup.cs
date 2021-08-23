using API.Extensions;
using API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace API
{
  public class Startup
  {
    private readonly IConfiguration _config;

    public Startup(IConfiguration config)
    {
      _config = config;
    }


    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      _ = services.AddApplicationServices(_config);
      _ = services.AddControllers();
      _ = services.AddCors();
      _ = services.AddSwaggerGen(c =>
        {
          c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
        });
      _ = services.AddIdentityServices(_config);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      _ = app.UseMiddleware<ExceptionMiddleware>();
      if (env.IsDevelopment())
      {
        _ = app.UseSwagger();
        _ = app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
      }

      _ = app.UseHttpsRedirection();

      _ = app.UseRouting();
      _ = app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));
      _ = app.UseAuthentication();
      _ = app.UseAuthorization();

      _ = app.UseEndpoints(endpoints =>
         endpoints.MapControllers()
       );

    }
  }
}
