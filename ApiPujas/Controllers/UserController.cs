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

                // =========================
                // UPDATE FIELDS
                // =========================
                user.Name = dto.Name;
                user.Email = dto.Email;
                user.Phone = dto.Phone;
                user.Address = dto.Address;
                user.Photo = dto.Photo;

                // =========================
                // PASSWORD OPTIONAL UPDATE
                // =========================
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