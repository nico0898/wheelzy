namespace api.Entities;

public class Car
{
    public int Id { get; set; }
    public int CarSpecId { get; set; }
    public int LocationId { get; set; }

    public CarSpec CarSpec { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    public ICollection<CarStatus> StatusHistory { get; set; } = new List<CarStatus>();
}
