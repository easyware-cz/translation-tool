using src.api.dtos;
using src.data.entities;

namespace src.data.mapping;

public static class EnvMapping
{
    public static EnvDto ToDto(this Env env)
    {
        return new(
            env.Id,
            env.Name
        );
    }

    public static Env ToEntity(this EnvNewDto env, int projectId)
    {
        return new Env()
        {
            ProjectId = projectId,
            Name = env.Name
        };
    }

    public static Env ToEntity(this EnvNewDto env, int projectId, int id)
    {
        return new Env()
        {
            Id = id,
            ProjectId = projectId,
            Name = env.Name            
        };
    }
}
