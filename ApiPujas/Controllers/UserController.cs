using ApiPujas.Data;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ApiPujas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET ALL USERS
        // =========================================
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

        // =========================================
        // CREATE USER
        // =========================================
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

                // 🔐 AQUÍ SE HACE EL HASH (NO EN ANGULAR)
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { isSuccess = true, data = user });
        }

        // =========================================
        // SEARCH USERS
        // =========================================
        [HttpGet("GetUsersByTerm/{searchTerm}")]
        public async Task<IActionResult> GetUsersByTerm(string searchTerm)
        {
            try
            {
                int.TryParse(searchTerm, out int idBusqueda);

                var users = await _context.Users
                    .AsNoTracking()
                    .Where(u =>
                        u.Name.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm) ||
                        u.Phone.Contains(searchTerm) ||
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

        [HttpPut("UpdateUser/{id}")]
        public ResponseDto UpdateUser(int id, [FromBody] User updatedUser)
        {
            try
            {
                // 1. Buscar el usuario existente en la base de datos
                var userDb = _context.Users.FirstOrDefault(u => u.Id == id);

                if (userDb == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Usuario no encontrado.";
                    return _response;
                }

                // 2. Limpiar validaciones de navegación (igual que en el Post)
                ModelState.Remove("Products");
                ModelState.Remove("Bids");

                // 3. Actualizar los campos permitidos
                userDb.Name = updatedUser.Name;
                userDb.Email = updatedUser.Email;
                userDb.Phone = updatedUser.Phone;
                userDb.Address = updatedUser.Address;

                // Solo actualizamos la foto si se envía una nueva (no es null)
                if (!string.IsNullOrEmpty(updatedUser.Photo))
                {
                    userDb.Photo = updatedUser.Photo;
                }

                // 4. Lógica opcional para la contraseña
                // Si el campo Password no viene vacío, significa que el usuario quiere cambiarla
                if (!string.IsNullOrEmpty(updatedUser.Password) && !updatedUser.Password.StartsWith("$2a$"))
                {
                    userDb.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
                }

                _context.Users.Update(userDb);
                _context.SaveChanges();

                // Limpiamos el password de la respuesta por seguridad
                userDb.Password = null;
                _response.Data = userDb;
                _response.IsSuccess = true;
                _response.Message = "Usuario actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "Error al actualizar: " + (ex.InnerException?.Message ?? ex.Message);
            }
            return _response;
        }

        // =========================================
        // LOGIN
        // =========================================
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
    }
}