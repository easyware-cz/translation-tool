using System.ComponentModel.DataAnnotations;

namespace src.api.dtos
{
    public record class KeyNewDto(
        [Required] string Name,
        string? Description
    );
}
