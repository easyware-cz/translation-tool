using src.api.dtos;
using src.data.entities;

namespace src.data.mapping;

public static class KeyMapping
{
        public static KeyDto ToDto(this Key key)
    {
        return new(
            key.Id,
            key.Name,
            key.Description
        );
    }

    public static Key ToEntity(this KeyNewDto key, int envId)
    {
        return new Key()
        {
            Name = key.Name,
            Description = key.Description,
            EnvId = envId
        };
    }

    public static Key ToEntity(this KeyNewDto key, int envId, int id)
    {
        return new Key()
        {
            Id = id,
            Name = key.Name,
            Description = key.Description,
            EnvId = envId
        };
    }
}
