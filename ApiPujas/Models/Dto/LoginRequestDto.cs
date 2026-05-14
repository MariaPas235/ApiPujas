namespace ApiPujas.Models.Dto
{
    /// <summary>
    /// DTO con las credenciales necesarias para autenticar a un usuario
    /// mediante email y contraseña.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Dirección de correo electrónico con la que el usuario está registrado.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Contraseña en texto plano. Se verifica contra el hash BCrypt almacenado en base de datos.
        /// </summary>
        public string Password { get; set; }
    }
}