namespace api.Entities;

public class Location
{
    public int Id { get; set; }
    public string ZipCode { get; set; } = null!;
    public ICollection<Car> Cars { get; set; } = new List<Car>();
    public ICollection<LocationBuyer> LocationBuyers { get; set; } = new List<LocationBuyer>();
}