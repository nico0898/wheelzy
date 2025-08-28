public class Status
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool RequiresStatusDate { get; set; }   // “Picked Up” => true
    public ICollection<CarStatus> CarStatuses { get; set; } = new List<CarStatus>();
}