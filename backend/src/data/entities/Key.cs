namespace src.data.entities;

public class Key
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public required int EnvId { get; set; }
    public Env? Env { get; set; }

    public IList<Translation>? Translations { get; set; }
}
