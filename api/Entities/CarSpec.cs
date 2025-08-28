namespace api.Entities;

public class CarSpec
{
    public int Id { get; set; }
    public int Year { get; set; }                 // Se mapeará a columna [Year] o CarYear
    public int MakeId { get; set; }
    public int ModelId { get; set; }
    public int? SubModelId { get; set; }

    public Make Make { get; set; } = null!;
    public Model Model { get; set; } = null!;
    public SubModel? SubModel { get; set; }
    public ICollection<Car> Cars { get; set; } = new List<Car>();
}