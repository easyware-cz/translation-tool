using System.ComponentModel.DataAnnotations;

namespace src.api.dtos
{
    public record class ProjectNewDto(
        [Required] string Name
        //int[] ProjectAdmins
    );
}
