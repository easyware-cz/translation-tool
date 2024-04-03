namespace src.api.dtos;

public record class NewTranslationDto(
    string Language,
    string Translation,
    string ModifiedBy
);

