using System;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Relación con Product
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Relación con User (Buyer)
        [Required]
        public int BuyerId { get; set; }
        public User Buyer { get; set; }
    }
}