namespace api.Entities;

public class Offer
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int BuyerId { get; set; }
    public decimal Amount { get; set; }
    public bool Current { get; set; }
    public DateTime Date { get; set; }

    public Car Car { get; set; } = null!;
    public Buyer Buyer { get; set; } = null!;
}