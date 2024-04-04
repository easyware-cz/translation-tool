using Microsoft.EntityFrameworkCore;
using src.data.db;
using src.data.mapping;

namespace src.api.endpoints;

public static class LanguagesEndpoints
{
    public static RouteGroupBuilder MapLanguagesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("languages");

        // GET /languages
        group.MapGet("/", async (TranslationContext dbContext) =>
            await dbContext.Languages
            .Select(language => language.ToDto())
            .AsNoTracking()
            .ToListAsync()
        );

        return group;
    }
}
