namespace api.Entities;

public class Buyer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<LocationBuyer> LocationBuyers { get; set; } = new List<LocationBuyer>();
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}
