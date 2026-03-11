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
        private ResponseDto _response;

        public UserController(AppDbContext context)
        {
            _context = context;
            _response = new ResponseDto();
        }

        [HttpGet("GetUsers")]
        public ResponseDto GetUsers()
        {
            try
            {
                var users = _context.Users.ToList();
                _response.Data = users;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("PostUser")]
        public ResponseDto PostUsers([FromBody] User user)
        {
            try
            {
                // Forzamos la limpieza de las propiedades de navegación para que el validador no se queje
                ModelState.Remove("Products");
                ModelState.Remove("Bids");

                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Datos inválidos";
                    return _response;
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _context.Users.Add(user);
                _context.SaveChanges();

                user.Password = null;
                _response.Data = user;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                // Aquí verás el error real si falta una columna o el email está duplicado
                _response.Message = "Error: " + (ex.InnerException?.Message ?? ex.Message);
            }
            return _response;
        }

        [HttpGet("GetUsersByTerm/{searchTerm}")]
        public ResponseDto GetUsersByTerm(string searchTerm)
        {
            try
            {
                int.TryParse(searchTerm, out int idBusqueda);

                var users = _context.Users.Where(u =>
                    u.Name.Contains(searchTerm) ||
                    u.Email == searchTerm ||
                    u.Phone == searchTerm ||
                    u.Id == idBusqueda)
                    .ToList();

                if (users == null || !users.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "No se encontraron usuarios.";
                    return _response;
                }

                _response.IsSuccess = true;
                _response.Data = users;
                _response.Message = $"Se encontraron {users.Count} resultados.";
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("Login")]
        public ResponseDto Login([FromBody] LoginRequestDto login)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == login.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Email o contraseña incorrectos";
                    return _response;
                }

                _response.IsSuccess = true;
                _response.Data = user;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}