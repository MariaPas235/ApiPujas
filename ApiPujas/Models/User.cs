using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiPujas.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public decimal Reputation { get; set; }

        public bool IsVerified { get; set; }

        public string? Photo { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

   

        // Relaciones
        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new List<Product>();

        [JsonIgnore]
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}