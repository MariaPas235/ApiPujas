using ApiPujas.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ApiPujas.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Required]
        public PurchaseState purchaseState { get; set; }

        public int OperationId { get; set; }

        [MaxLength(500)]
        public string? Data { get; set; }

        [MaxLength(100)]
        public string? OrderNumber { get; set; }

        // Buyer
        [Required]
        public int BuyerId { get; set; }
        public User Buyer { get; set; }

        // Product
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}