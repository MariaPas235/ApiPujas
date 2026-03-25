using ApiPujas.Data;
using ApiPujas.Services;
using ApiPujas.Hubs; // Asegúrate de tener la carpeta Hubs creada
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
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. SignalR (Añadido)
            builder.Services.AddSignalR();

            // 3. CORS Configurado específicamente para SignalR
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200") // Tu URL de Angular
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // 👈 OBLIGATORIO para SignalR
                });
            });

            // 4. Controllers + enums como string
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // 5. Servicio de subastas automáticas
            builder.Services.AddHostedService<AuctionBackgroundService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configuración del Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Usar la política CORS antes de Authorization y MapHub
            app.UseCors("AllowAngular");

            app.UseAuthorization();

            // 6. Mapeo de rutas (Añadido)
            app.MapControllers();
            app.MapHub<AuctionHub>("/auctionHub"); // 👈 Esta es la URL que pondrás en Angular

            app.Run();
        }
    }
}