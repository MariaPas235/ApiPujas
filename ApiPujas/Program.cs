using ApiPujas.Data;
using ApiPujas.Services;
using ApiPujas.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ApiPujas
{
    /// <summary>
    /// Punto de entrada de la aplicación. Configura y arranca el servidor ASP.NET Core
    /// con todos los servicios, middlewares y endpoints necesarios para la API de subastas.
    /// </summary>
    public class Program
    {

        /// <summary>
        /// Método principal que construye y ejecuta la aplicación.
        /// El pipeline de configuración sigue este orden:
        /// <list type="number">
        ///   <item><description><b>DbContext</b>: SQL Server con reintentos automáticos ante fallos de conexión.</description></item>
        ///   <item><description><b>HttpClient</b>: Cliente HTTP inyectable, requerido por <c>BizumController</c>.</description></item>
        ///   <item><description><b>SignalR</b>: Soporte de comunicación en tiempo real para el hub de subastas.</description></item>
        ///   <item><description><b>CORS</b>: Política <c>AllowAngular</c> que permite peticiones desde <c>http://localhost:4200</c>.</description></item>
        ///   <item><description><b>Controllers</b>: Serialización de enums como strings en las respuestas JSON.</description></item>
        ///   <item><description><b>AuctionBackgroundService</b>: Servicio en segundo plano para gestionar el ciclo de vida de las subastas.</description></item>
        ///   <item><description><b>Swagger</b>: Documentación interactiva de la API, disponible solo en entorno de desarrollo.</description></item>
        /// </list>
        /// </summary>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. DbContext y HTTPClient
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

            builder.Services.AddHttpClient(); 

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

            // 6. Middlewares
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