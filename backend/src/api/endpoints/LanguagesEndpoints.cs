using Microsoft.EntityFrameworkCore;
using src.api.dtos;
using src.data.db;
using src.data.entities;
using src.data.mapping;
using static src.api.methods.EndpointsMethods;

namespace src.api.endpoints;

public static class LanguagesEndpoints
{
    const string LANGUAGEROUTE = "GetLanguage";
    public static RouteGroupBuilder MapLanguagesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("languages");

        // GET /languages
        group.MapGet("/", async (TranslationContext dbContext) =>
        {
            var languages = await GetLanguagesAsync(dbContext);
            
            return Results.Ok(languages.Select(l => l.ToDto()).ToList());
        });

        // GET /languages/{id}
        group.MapGet("/{id}", async (int id, TranslationContext dbContext) =>
        {
            Language? language = await dbContext.Languages.FindAsync(id);

            return language is null
                ? Results.NotFound() : Results.Ok(language.ToDto());
        })
            .WithName(LANGUAGEROUTE);

        // POST /languages
        group.MapPost("/", async (LanguageNewDto newLanguageDto, TranslationContext dbContext) =>
        {
            Language newLanguage = newLanguageDto.ToEntity();
            await dbContext.Languages.AddAsync(newLanguage);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(LANGUAGEROUTE, new { id = GetLanguageAsync(newLanguage.LanguageCode, dbContext) }, newLanguage.ToDto());
        });

        // PUT /languages/{id}
        group.MapPut("/{id}", async (int id, LanguageNewDto updatedLanguageDto, TranslationContext dbContext) =>
        {
            // var currentLanguage = await dbContext.Languages.FindAsync(id);
            // if (currentLanguage is null) return Results.NotFound();

            // //dbContext.Entry(currentLanguage).CurrentValues.SetValues(updatedLanguageDto.ToEntity(id));
            // currentLanguage.LanguageCode = updatedLanguageDto.LanguageCode;
            // await dbContext.SaveChangesAsync();

            // return Results.Ok();

            int count = await dbContext.Languages.Where(l => l.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(l => l.LanguageCode, updatedLanguageDto.LanguageCode));

            return count is 0
                ? Results.NotFound() : Results.Ok();
        });

        // DELETE /languages/{id}
        group.MapDelete("/{id}", async (int id, TranslationContext dbContext) =>
        {
            int count = await dbContext.Languages.Where(language => language.Id == id)
                .ExecuteDeleteAsync();

            return count is 0
                ? Results.NotFound() : Results.Ok(count);
        });

        return group;
    }
}
