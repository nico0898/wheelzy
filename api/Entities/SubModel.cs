namespace api.Entities;

public class SubModel
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string Name { get; set; } = null!;
    public Model Model { get; set; } = null!;
}