using ApiPujas.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal InitialPrice { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public ProductState productState { get; set; }

        public string? Photo { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        // Relación con Seller
        [Required]
        public int SellerId { get; set; }
        public User Seller { get; set; }

        // Relación con Bids
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}