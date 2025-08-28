namespace  api.Entities;

public class Make
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Model> Models { get; set; } = new List<Model>();
}