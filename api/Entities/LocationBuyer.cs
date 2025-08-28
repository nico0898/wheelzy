namespace api.Entities;

public class LocationBuyer
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public int BuyerId { get; set; }

    public Location Location { get; set; } = null!;
    public Buyer Buyer { get; set; } = null!;
}