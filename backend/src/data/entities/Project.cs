namespace src.data.entities;

public interface IHasName
{
    string Name { get; set; }
}
public class Project : IHasName
{
    public int Id { get; set; }
    public required string Name { get; set; }
    //public int[] ProjectAdmins { get; set; } = [];

    public IList<Env>? Env { get; set; }
}
