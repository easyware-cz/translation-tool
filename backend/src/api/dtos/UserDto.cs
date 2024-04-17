namespace src.api.dtos
{
    public record class UserDto(
        int Id,
        string Email,
        string Password
    );
}
