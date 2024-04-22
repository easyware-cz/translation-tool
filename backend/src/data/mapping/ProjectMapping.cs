using src.api.dtos;
using src.data.entities;

namespace src.data.mapping;

public static class ProjectMapping
{
        public static ProjectDto ToDto(this Project project)
    {
        return new(
            project.Id,
            project.Name
        );
    }

    public static Project ToEntity(this ProjectNewDto project)
    {
        return new Project()
        {
            Name = project.Name
        };
    }

    public static Project ToEntity(this ProjectNewDto project, int id)
    {
        return new Project()
        {
            Id = id,
            Name = project.Name
        };
    }
}
