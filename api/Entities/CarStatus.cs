using api.Entities;

public class CarStatus
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int StatusId { get; set; }
    public DateTime? StatusDate { get; set; }      // Obligatoria si RequiresStatusDate = true (se valida en app)
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public bool IsCurrent { get; set; }

    public Car Car { get; set; } = null!;
    public Status Status { get; set; } = null!;
}