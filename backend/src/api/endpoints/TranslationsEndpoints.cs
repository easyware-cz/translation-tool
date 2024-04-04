using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using src.api.dtos;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class TranslationsEndpoints
{
    const string GetTranslationEndpointName = "GetTranslation";

    public static RouteGroupBuilder MapTranslationsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations
        group.MapGet("/", async (TranslationContext dbContext) =>
        await dbContext.Translations
            .Include(translation => translation.LanguageCode)  //game => game.Genre
            .Select(translation => translation.ToTranslationDto()) //ToTranslationSummaryDto
            .AsNoTracking() // no tracking improve performance
            .ToListAsync()
        );

        // GET /translations/{id}
        group.MapGet("/{id}", async (int id, TranslationContext dbContext) =>
        {
            Translation? translation = await dbContext.Translations.FindAsync(id);

            return translation is null ? Results.NotFound() : Results.Ok(translation.ToTranslationDto());
        })
            .WithName(GetTranslationEndpointName);

        // POST /translations
        group.MapPost("/", async (NewTranslationDto newTranslation, TranslationContext dbContext) =>
        {
            Translation translation = newTranslation.ToEntity();
            //translation.LanguageCode = dbContext.Languages.Find(newTranslation.Language_id); // ToEntity method does not provide associated fields mapping
            dbContext.Translations.Add(translation);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetTranslationEndpointName, new { id = translation.Id }, translation.ToTranslationDto());
        });

        // PUT /translations/{id}
        group.MapPut("/{id}", async (int id, NewTranslationDto updatedTranslation, TranslationContext dbContext) =>
        {
            var currentTranslation = await dbContext.Translations.FindAsync(id);

            if (currentTranslation == null) return Results.NotFound();

            dbContext.Entry(currentTranslation).CurrentValues.SetValues(updatedTranslation.ToEntity(id));
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });


        // DELETE /translations/{id}
        group.MapDelete("/{id}", async (int id, TranslationContext dbContext) =>
        {
            await dbContext.Translations.Where(translation => translation.Id == id).ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}
