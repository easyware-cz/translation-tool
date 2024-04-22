using System.ComponentModel.DataAnnotations;

namespace src.api.dtos
{
    public record class TranslationNewDto(
        string KeyName,
        [Required] string TranslationText
    );

}
