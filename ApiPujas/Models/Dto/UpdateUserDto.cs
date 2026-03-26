public class UpdateUserDto
{
    public int Id { get; set; }

    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Photo { get; set; }

    // opcional (solo si quieres cambiar contraseña)
    public string? Password { get; set; }
}