namespace api.DTOs;

public sealed class CarSummaryDTO
{
    public int CarId { get; set; }
    public int CarYear { get; set; }
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string? SubModel { get; set; }
    public string? CurrentBuyer { get; set; }
    public decimal? CurrentQuote { get; set; }
    public string? CurrentStatus { get; set; }
    public DateTime? CurrentStatusDate { get; set; }
}
