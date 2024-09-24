using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RPPP_WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RPPP_WebApp
{
  public static class StartupExtensions
  {
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
      builder.Services.AddDbContext<Rppp12Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RPPP12")));
      builder.Services.AddControllersWithViews();
      builder.Services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "RPPP12 Web API",
        Version = "v1"
      });
      c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()); //This line
      
      var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
      var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

      c.IncludeXmlComments(xmlPath);
      });

      builder.Services.AddScoped<RPPP_WebApp.Controllers.CommonController<int, RPPP_WebApp.ViewModels.PlantViewModel>, RPPP_WebApp.Controllers.PlantsController>();
      builder.Services.AddScoped<RPPP_WebApp.Controllers.CommonController<int, RPPP_WebApp.ViewModels.OrderViewModel>, RPPP_WebApp.Controllers.OrderController>();
      builder.Services.AddScoped<RPPP_WebApp.Controllers.CommonController<int, RPPP_WebApp.ViewModels.OperationViewModel>, RPPP_WebApp.Controllers.OperationController>();

      return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
      #region Needed for nginx and Kestrel (do not remove or change this region)
      app.UseForwardedHeaders(new ForwardedHeadersOptions
      {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                           ForwardedHeaders.XForwardedProto
      });
      string pathBase = app.Configuration["PathBase"];
      if (!string.IsNullOrWhiteSpace(pathBase))
      {
        app.UsePathBase(pathBase);
      }
      #endregion

      if (app.Environment.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      app.UseSwagger();
      app.UseSwaggerUI(c => {
      c.SwaggerEndpoint("/rppp/12/swagger/v1/swagger.json",
        "RPPP12 WebAPI");
        c.RoutePrefix = "docs";
      });

      app.UseStaticFiles()
         .UseRouting()
         .UseEndpoints(endpoints =>
         {
           endpoints.MapDefaultControllerRoute();
         });

      return app;
    }
  }
}