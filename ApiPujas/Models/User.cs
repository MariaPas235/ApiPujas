using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiPujas.Models
{
    /// <summary>
    /// Entidad que representa un usuario registrado en la plataforma de subastas.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre completo del usuario. Máximo 100 caracteres.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Dirección de correo electrónico del usuario. Debe ser única y válida. Máximo 150 caracteres.
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        /// <summary>
        /// Hash de la contraseña del usuario. Nunca se almacena en texto plano.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Número de teléfono de contacto del usuario. Máximo 20 caracteres. Campo opcional.
        /// </summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Fecha y hora en UTC en que el usuario se registró en la plataforma. Por defecto, el momento actual.
        /// </summary>
        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Reputación acumulada del usuario basada en las valoraciones recibidas como vendedor.
        /// </summary>
        public decimal Reputation { get; set; }

        /// <summary>
        /// Indica si el usuario ha verificado su identidad en la plataforma.
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// URL o ruta de la foto de perfil del usuario. Campo opcional.
        /// </summary>
        public string? Photo { get; set; }

        /// <summary>
        /// Dirección postal del usuario. Máximo 250 caracteres. Campo opcional.
        /// </summary>
        [MaxLength(250)]
        public string? Address { get; set; }

        /// <summary>
        /// Descriptor facial del usuario utilizado para verificación biométrica. Campo opcional.
        /// </summary>
        public string? FaceDescriptor { get; set; }

        /// <summary>
        /// Colección de productos publicados en subasta por el usuario como vendedor.
        /// </summary>
        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();

        /// <summary>
        /// Colección de pujas realizadas por el usuario como comprador.
        /// </summary>
        [JsonIgnore]
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}