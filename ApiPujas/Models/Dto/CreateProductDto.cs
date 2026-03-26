public class CreateProductDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal InitialPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Photo { get; set; }
    public string Category { get; set; }

    public int SellerId { get; set; }
}