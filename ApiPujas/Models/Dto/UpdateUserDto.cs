/// <summary>
/// DTO para la actualización completa de los datos de un usuario.
/// </summary>
public class UpdateUserDto
{

    /// <summary>
    /// Identificador único del usuario a actualizar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Dirección de correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; }


    /// <summary>
    /// Número de teléfono de contacto del usuario.
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Dirección postal del usuario.
    /// </summary>
    public string Address { get; set; }


    /// <summary>
    /// URL o ruta de la foto de perfil del usuario.
    /// </summary>
    public string Photo { get; set; }

    /// <summary>
    /// Nueva contraseña del usuario. Campo opcional; solo se actualiza si se proporciona un valor.
    /// </summary>
    public string? Password { get; set; }
}