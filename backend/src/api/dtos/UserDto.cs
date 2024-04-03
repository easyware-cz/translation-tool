namespace src.api.dtos;

public record class UserDto(
    int User_id,
    string Email,
    string Password
);
