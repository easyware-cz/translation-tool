using System.ComponentModel.DataAnnotations;

namespace src.api.dtos
{
    public record class EnvNewDto(
        [Required] string Name
        //int[] Translators
    );
}
