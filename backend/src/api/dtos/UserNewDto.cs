using System.ComponentModel.DataAnnotations;

namespace src.api.dtos
{
    public record class UserNewDto(
        [Required] string Email,
        [Required] string Password
    );
}
