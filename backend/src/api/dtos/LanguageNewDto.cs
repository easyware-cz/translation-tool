using System.ComponentModel.DataAnnotations;

namespace src.api.dtos;

public record class LanguageNewDto(
    [Required] string LanguageCode
);
