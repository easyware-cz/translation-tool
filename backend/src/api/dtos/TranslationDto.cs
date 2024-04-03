namespace src.api.dtos;

public record class TranslationDto(
    //string Project,
    //string Environment,
    int Id,
    string Language,
    //string Key,
    //string Value
    //string Description,
    string Translation,
    DateTime ModifiedAt,
    string ModifiedBy
);