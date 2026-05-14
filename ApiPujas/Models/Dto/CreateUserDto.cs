/// <summary>
/// DTO con los datos necesarios para registrar un nuevo usuario en la plataforma.
/// La contraseña se recibe en texto plano y es hasheada con BCrypt en el servidor
/// antes de persistirse; nunca se almacena en texto plano.
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Dirección de correo electrónico. Debe ser única en la plataforma.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Contraseña en texto plano. Se hashea con BCrypt en el servidor antes de almacenarse.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Número de teléfono de contacto del usuario.
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Dirección postal del usuario, utilizada para la gestión de envíos.
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// URL o ruta de la foto de perfil del usuario.
    /// </summary
    public string Photo { get; set; }
}