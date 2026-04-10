namespace ApiPujas.DTOs
{
    public class CreateReviewDto
    {
        public int PurchaseId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
    }
}