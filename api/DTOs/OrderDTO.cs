namespace api.DTOs;

public class OrderDTO
{ 
    public int OfferId { get; set; }
    public int CarId { get; set; }
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsActive { get; set; }           // Offer.Current
    public int? StatusId { get; set; }           // estado ACTUAL del auto
    public string? StatusName { get; set; }
}