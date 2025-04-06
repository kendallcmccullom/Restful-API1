using Hellang.Middleware.ProblemDetails;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace HelloPlatform
{
    /// <summary>
    /// Main program class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// ASP.NET Core entry point.
        /// </summary>
        /// <param name="args">Startup arguments.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Generate API info and description
                // https://aka.ms/aspnetcore/swashbuckle?view=aspnetcore-6.0&tabs=visual-studio#api-info-and-description
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Hello.Platform API", // Update with your name or project name :)
                    Description = "Summer Internship 2024: Week 2 project"
                });

                // Include XML documentation comments in Swagger
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.AddProblemDetails();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseProblemDetails(); // Wraps exceptions for responses


            app.MapControllers();

            app.Run();
        }
    }
}
