namespace src.api.dtos
{
    public record class EnvFullDto(
        int Id,
        string Name,
        HashSet<string> Localizations
    );
}
