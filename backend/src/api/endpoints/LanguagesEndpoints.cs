using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using src.api.dtos;
using src.api.methods;
using static src.api.utilities.EndpointsMethods;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class LanguagesEndpoints
{
    const string LANGUAGEROUTE = "GetLanguage";
    public static RouteGroupBuilder MapLanguagesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("languages").WithParameterValidation();

        // GET /languages
        group.MapGet("/",
            async Task<Ok<List<LanguageDto>>>
            (TranslationContext dbContext) =>
            {
                var languages = await GetLanguagesAsync(dbContext);
                
                return TypedResults.Ok(languages.Select(l => l.ToDto()).ToList());
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Language"}],
                OperationId = "GetLanguages",
                Summary = "Get a list of all languages",
                Description = "Response body: List of LanguageDto"
            });

        // GET /languages/{id}
        group.MapGet("/{id}",
            async Task<Results<NotFound<string>, Ok<LanguageDto>>>
            (int id, TranslationContext dbContext) =>
            {
                var language = await dbContext.Languages.FindAsync(id);
                if (language is null)
                    return TypedResults.NotFound(Message.LanguageNotFound(id));
                else
                    return TypedResults.Ok(language.ToDto());
            })
            .WithName(LANGUAGEROUTE)
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Language"}],
                OperationId = "GetLanguage",
                Summary = "Get a language by ID",
                Description = "Response body: LanguageDto"
            });

        // POST /languages
        group.MapPost("/",
            async Task<Results<BadRequest<string>, CreatedAtRoute<LanguageDto>>>
            (LanguageNewDto newLanguageDto, TranslationContext dbContext) =>
            {
                Language newLanguage = newLanguageDto.ToEntity();

                var sameLanguage = await GetLanguageAsync(newLanguage.LanguageCode, dbContext);
                if (sameLanguage is not null)
                    return TypedResults.BadRequest(Message.LanguageAlreadyExists(newLanguage.LanguageCode));
                    
                await dbContext.Languages.AddAsync(newLanguage);
                await dbContext.SaveChangesAsync();

                return TypedResults.CreatedAtRoute(
                    routeName: LANGUAGEROUTE,
                    routeValues: new { id = newLanguage.Id },
                    value: newLanguage.ToDto());
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Language"}],
                OperationId = "PostLanguage",
                Summary = "Add a new language",
                Description = "Request body: LanguageNewDto<br />Response body: LanguageDto"
            });

        // PUT /languages/{id}
        group.MapPut("/{id}",
            async Task<Results<BadRequest<string>, NotFound<string>, Ok>>
            (int id, LanguageNewDto updatedLanguageDto, TranslationContext dbContext) =>
            {
                Language updatedLanguage = updatedLanguageDto.ToEntity();
                var sameLanguage = await GetLanguageAsync(updatedLanguage.LanguageCode, dbContext);
                if (sameLanguage is not null)
                    return TypedResults.BadRequest(Message.LanguageAlreadyExists(updatedLanguage.LanguageCode));

                int count = await dbContext.Languages.Where(l => l.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(l => l.LanguageCode, updatedLanguage.LanguageCode));
                if (count is 0)
                    return TypedResults.NotFound(Message.LanguageNotFound(id));
                else
                    return TypedResults.Ok();
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Language"}],
                OperationId = "PutLanguage",
                Summary = "Update the language",
                Description = "Request body: LanguageNewDto<br />Response body: LanguageDto"
            });

        // DELETE /languages/{id}
        group.MapDelete("/{id}",
            async Task<Results<NotFound<string>, Ok<int>>>
            (int id, TranslationContext dbContext) =>
            {
                int count = await dbContext.Languages.Where(l => l.Id == id).ExecuteDeleteAsync();
                if (count is 0)
                    return TypedResults.NotFound(Message.LanguageNotFound(id));
                else
                    return TypedResults.Ok(count);
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Language"}],
                OperationId = "DeleteLanguage",
                Summary = "Delete the language",
                Description = ""
            });

        return group;
    }
}
