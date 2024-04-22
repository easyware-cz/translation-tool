namespace src.data.entities;

public class Env // Environment is key word in C#
{
    public int Id { get; set; }
    public required string Name { get; set; }
    //public int[] Translators { get; set; } = [];
        
    public required int ProjectId { get; set; } // FK to Project.Id
    public Project? Project { get; set; } // for M:1 accociation with the Project

    public IList<Key>? Key { get; set; }
}
