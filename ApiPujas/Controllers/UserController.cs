using ApiPujas.Data;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ApiPujas.Controllers
{

    /// <summary>
    /// Controlador para gestionar los usuarios de la plataforma.
    /// Cubre el registro, autenticación (por contraseña y por reconocimiento facial),
    /// consulta, actualización y almacenamiento del descriptor facial.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor del controlador UserController.
        /// </summary>
        /// <param name="context">Contexto de base de datos de la aplicación.</param>
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la lista completa de usuarios registrados en la plataforma.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Lista de usuarios con <c>isSuccess</c> y <c>data</c>.</description></item>
        ///   <item><description><c>500 Internal Server Error</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(new
                {
                    isSuccess = true,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        public class LoginWithFaceRequest
        {
            public List<float> FaceDescriptor { get; set; } = new();
        }

        /// <summary>
        /// Autentica a un usuario comparando el descriptor facial recibido contra los descriptores
        /// almacenados en base de datos mediante distancia euclidiana.
        /// Se considera un match válido si la distancia mínima encontrada es inferior al umbral de 0.6.
        /// </summary>
        /// <param name="request">Objeto que contiene el descriptor facial como lista de valores flotantes.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK + isSuccess = true</c>: Rostro reconocido; devuelve el ID del usuario.</description></item>
        ///   <item><description><c>200 OK + isSuccess = false</c>: Ningún rostro registrado supera el umbral de similitud.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("LoginWithFace")]
        public async Task<IActionResult> LoginWithFace([FromBody] LoginWithFaceRequest request)
        {
            var users = await _context.Users
                .Where(u => u.FaceDescriptor != null)
                .ToListAsync();

            User? matchedUser = null;
            double bestDistance = double.MaxValue;
            const double THRESHOLD = 0.6; 

            foreach (var user in users)
            {
                var savedDescriptor = System.Text.Json.JsonSerializer
                    .Deserialize<List<float>>(user.FaceDescriptor!);

                if (savedDescriptor == null) continue;
                double distance = EuclideanDistance(request.FaceDescriptor, savedDescriptor);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    matchedUser = user;
                }
            }
            if (matchedUser != null && bestDistance < THRESHOLD)
            {
                return Ok(new
                {
                    isSuccess = true,
                    data = new { id = matchedUser.Id }
                });
            }

            return Ok(new { isSuccess = false });
        }

        /// <summary>
        /// Calcula la distancia euclidiana entre dos descriptores faciales representados
        /// como vectores de valores flotantes. Se usa como métrica de similitud en el login facial.
        /// </summary>
        /// <param name="a">Primer descriptor facial.</param>
        /// <param name="b">Segundo descriptor facial.</param>
        /// <returns>Distancia euclidiana entre ambos vectores; cuanto menor, mayor similitud.</returns>
        private double EuclideanDistance(List<float> a, List<float> b)
        {
            double sum = 0;
            for (int i = 0; i < Math.Min(a.Count, b.Count); i++)
            {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Registra un nuevo usuario en la plataforma.
        /// La contraseña se almacena como hash BCrypt; nunca se guarda en texto plano.
        /// </summary>
        /// <param name="dto">Datos del nuevo usuario: nombre, email, teléfono, dirección, foto y contraseña.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Usuario creado correctamente, devuelve la entidad persistida.</description></item>
        ///   <item><description><c>400 Bad Request</c>: El modelo de datos no es válido.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("PostUser")]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Photo = dto.Photo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { isSuccess = true, data = user });
        }

        /// <summary>
        /// Busca un usuario por su identificador numérico exacto.
        /// </summary>
        /// <param name="searchTerm">Cadena de texto que se convierte a entero para buscar por ID.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Usuario encontrado con <c>isSuccess</c>, <c>count</c> y <c>data</c>.</description></item>
        ///   <item><description><c>404 Not Found</c>: Ningún usuario coincide con el ID indicado.</description></item>
        ///   <item><description><c>500 Internal Server Error</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("GetUsersByID/{searchTerm}")]
        public async Task<IActionResult> GetUsersByTerm(string searchTerm)
        {
            try
            {
                int.TryParse(searchTerm, out int idBusqueda);

                var users = await _context.Users
                    .Where(u =>          
                        u.Id == idBusqueda)
                    .ToListAsync();

                if (!users.Any())
                {
                    return NotFound(new
                    {
                        isSuccess = false,
                        message = "No se encontraron usuarios"
                    });
                }

                return Ok(new
                {
                    isSuccess = true,
                    count = users.Count,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Busca usuarios cuyo nombre comience por el término de búsqueda indicado (búsqueda por prefijo).
        /// </summary>
        /// <param name="searchTerm">Prefijo del nombre a buscar.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Usuarios encontrados con <c>isSuccess</c>, <c>count</c> y <c>data</c>.</description></item>
        ///   <item><description><c>404 Not Found</c>: Ningún usuario coincide con el prefijo indicado.</description></item>
        ///   <item><description><c>500 Internal Server Error</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("GetUsersByName/{searchTerm}")]
        public async Task<IActionResult> GetUsersByName(string searchTerm)
        {
            try
            {
               

                var users = await _context.Users
                    .Where(u =>
                        u.Name.StartsWith(searchTerm))
                    .ToListAsync();

                if (!users.Any())
                {
                    return NotFound(new
                    {
                        isSuccess = false,
                        message = "No se encontraron usuarios"
                    });
                }

                return Ok(new
                {
                    isSuccess = true,
                    count = users.Count,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Actualiza los datos de perfil de un usuario existente.
        /// La contraseña solo se rehashea y actualiza si se proporciona un valor no vacío en el DTO.
        /// </summary>
        /// <param name="id">Identificador único del usuario a actualizar.</param>
        /// <param name="dto">Nuevos valores del perfil: nombre, email, teléfono, dirección, foto y contraseña opcional.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Usuario actualizado correctamente, devuelve la entidad modificada.</description></item>
        ///   <item><description><c>404 Not Found</c>: No existe ningún usuario con el ID indicado.</description></item>
        ///   <item><description><c>500 Internal Server Error</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        isSuccess = false,
                        message = "Usuario no encontrado"
                    });
                }
                user.Name = dto.Name;
                user.Email = dto.Email;
                user.Phone = dto.Phone;
                user.Address = dto.Address;
                user.Photo = dto.Photo;
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    isSuccess = true,
                    data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Autentica a un usuario mediante email y contraseña.
        /// Verifica la contraseña contra el hash BCrypt almacenado y,
        /// si es correcta, devuelve los datos del usuario omitiendo el hash de contraseña.
        /// </summary>
        /// <param name="login">Credenciales de acceso: email y contraseña en texto plano.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Autenticación correcta; devuelve los datos del usuario sin <c>PasswordHash</c>.</description></item>
        ///   <item><description><c>401 Unauthorized</c>: Email no registrado o contraseña incorrecta.</description></item>
        ///   <item><description><c>500 Internal Server Error</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto login)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == login.Email);

                if (user == null)
                {
                    return Unauthorized(new
                    {
                        isSuccess = false,
                        message = "Email o contraseña incorrectos"
                    });
                }

                bool passwordOk = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);

                if (!passwordOk)
                {
                    return Unauthorized(new
                    {
                        isSuccess = false,
                        message = "Email o contraseña incorrectos"
                    });
                }

                user.PasswordHash = null;

                return Ok(new
                {
                    isSuccess = true,
                    data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        public class SaveFaceRequest
        {
            public int UserId { get; set; }
            public List<float> FaceDescriptor { get; set; } = new();
        }

        /// <summary>
        /// Almacena el descriptor facial de un usuario serializado en formato JSON,
        /// sobreescribiendo cualquier descriptor previo.
        /// Este descriptor se utilizará posteriormente para el login facial.
        /// </summary>
        /// <param name="request">Objeto con el ID del usuario y su descriptor facial como lista de valores flotantes.</param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c>: Descriptor guardado correctamente.</description></item>
        ///   <item><description><c>404 Not Found</c>: No existe ningún usuario con el ID indicado.</description></item>
        ///   <item><description><c>500 Internal Server Error</c>: Error interno al acceder a la base de datos.</description></item>
        /// </list>
        /// </returns>
        [HttpPost("SaveFace")]
        public async Task<IActionResult> SaveFace([FromBody] SaveFaceRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                    return NotFound(new { isSuccess = false, message = "Usuario no encontrado" });

                user.FaceDescriptor = System.Text.Json.JsonSerializer.Serialize(request.FaceDescriptor);
                await _context.SaveChangesAsync();

                return Ok(new { isSuccess = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = ex.Message });
            }
        }
    }
}