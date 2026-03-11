namespace ApiPujas.DTOs
{
    public class CreateBidDto
    {
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public decimal Amount { get; set; }
    }
}