using ApiPujas.Data;
using ApiPujas.Models;
using ApiPujas.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace ApiPujas.Controllers
{
    [Route("/api/[Controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private ResponseDto _response;

        public UserController(AppDbContext context)
        {
            _context = context;
            _response = new ResponseDto();

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetUsers")]
        public ResponseDto GetUsers()
        {
            try
            {
                IEnumerable<User> users = _context.Users.ToList();
                _response.Data = users;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>

        [HttpPost("PostUser")]
        public ResponseDto PostUsers([FromBody] User user)
        {
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                _response.Data = user;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        /// <summary>
        /// Función para buscar un Usuario en la BD segun el parametro enviado, permite Name, Email, Phone y Id, este ultimo es casteado de String a int
        /// </summary>
        /// <param name="searchTerm"> Termino por el cual se encontrará el usuario (Name, Email, Phone, Id) </param>
        /// <returns></returns>
        /// 

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
                    _response.Data = null;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public ResponseDto Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == loginDto.Email.ToLower());

                if (user == null || user.Password != loginDto.Password)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Email o contraseña incorrectos.";
                    return _response;
                }

                _response.IsSuccess = true;
                _response.Data = user;
                _response.Message = "Login exitoso.";
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
