using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPujas.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Reputation { get; set; }

        public bool IsVerified { get; set; }

        public string Photo { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }

        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Bid> Bids { get; set; }
    }
}