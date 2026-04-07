using ApiPujas.Data;
using ApiPujas.Services;
using ApiPujas.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ApiPujas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }
                ));

            // --- NUEVO: Registro de HttpClient para que BizumController funcione ---
            builder.Services.AddHttpClient(); // <--- AÑADIR ESTA LÍNEA 🚀

            // 2. SignalR
            builder.Services.AddSignalR();

            // 3. CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // 4. Controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // 5. Otros servicios
            builder.Services.AddHostedService<AuctionBackgroundService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Middlewares
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAngular");
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<AuctionHub>("/auctionHub");

            app.Run();
        }
    }
}