using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using src.api.dtos;
using src.api.methods;
using static src.api.utilities.EndpointsMethods;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class TranslationsEndpoints
{
    const string TRANSLATIONROUTE = "GetTranslation";

    public static RouteGroupBuilder MapTranslationsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations/{project}/{environment}/{language}?format={json | resx}
        group.MapGet("/{projectName}/{envName}/{languageCode}",
            async Task<Results<NotFound<string>, Ok<List<TranslationDto>>>>
            (string projectName, string envName, string languageCode, TranslationContext dbContext, string? format = "json") =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));
        
                var language = await GetLanguageAsync(languageCode, dbContext);
                if (language is null)
                    return TypedResults.NotFound(Message.LanguageNotFound(languageCode));

                var translations = await GetTranslationsAsync(env, language, dbContext);
                if (translations is null)
                    return TypedResults.NotFound(Message.TranslationNotFound(envName, languageCode));
                else
                    return TypedResults.Ok(translations.OrderBy(t => t.KeyId).Select(t => t.ToDto()).ToList());
            })
            .WithName(TRANSLATIONROUTE)
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Translation"}],
                OperationId = "GetTranslations",
                Summary = "Get a list of all translations in the given language and environment",
                Description = "Response body: List of TranslationDto"
            });

        // POST /translations/{project}/{environment}/{languageCode}
        // For each Project/Env/Key create "empty" Translation (translationText = "") in Project/Env/Language
        // If any Translation already exists -> return
        // Option to add empty Translations only for newly added Keys from Env that are not part of Translations -> intersect between Env.Keys and Translations.Keys
        group.MapPost("/{projectName}/{envName}/{languageCode}",
            async Task<Results<NotFound<string>, BadRequest<string>, CreatedAtRoute<List<TranslationDto>>>>
            (string projectName, string envName, string languageCode, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));
        
                var language = await GetLanguageAsync(languageCode, dbContext);
                if (language is null)
                    return TypedResults.NotFound(Message.LanguageNotFound(languageCode));

                var keys = await GetKeysAsync(env, dbContext); // Get list of all Keys for given Project and Environment
                if (keys is null)
                    return TypedResults.NotFound(Message.KeysNotFound(projectName, envName) );
                
                if (await IsTranslationAsync(env, language, dbContext) is true)
                    return TypedResults.BadRequest(Message.TranslationsAlreadyExist(languageCode));
                
                Translation newTranslation;
                List<Translation> newTranslations = []; //var newTranslations = new List<Translation>();
                TranslationNewDto newTranslationDto; // input Dto
                List<TranslationDto> TranslationDtos = []; // output Dtos
                
                foreach (var key in keys)
                {
                    // newTranslation = new Translation
                    // {
                    //     TranslationText = string.Empty,
                    //     UpdatedAt = DateTime.UtcNow,
                    //     UpdatedBy = "user.Email",
                    //     KeyId = key.Id,
                    //     LanguageId = language.Id
                    // };
                    newTranslationDto = new(key.Name, string.Empty); // Translation text will be updated by a PUT request
                    newTranslation = newTranslationDto.ToEntity(key.Id, language.Id); // Adding UpdatedAt, UpdatedBy
                    newTranslations.Add(newTranslation);
                }

                await dbContext.Translations.AddRangeAsync(newTranslations);
                await dbContext.SaveChangesAsync();

                var translations = await GetTranslationsAsync(env, language, dbContext);

                return TypedResults.CreatedAtRoute(
                    routeName: TRANSLATIONROUTE,
                    routeValues: new { projectName, envName, languageCode },
                    value: translations.Select(t => t.ToDto()).ToList());
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Translation"}],
                OperationId = "PostTranslations",
                Summary = "Post a list of translations in the given language and environment",
                Description = "Request body: List of TranslationNewDto<br />Response body: List of TranslationDto"
            });

        // PUT /translations/{project}/{environment}/{language}?format={json / resx}
        // Add or update the translationText in given Translations
        group.MapPut("/{projectName}/{envName}/{languageCode}",
            async Task<Results<NotFound<string>, BadRequest<string>, Ok>>
            (string projectName, string envName, string languageCode, List<TranslationNewDto> updatedTranslationDtos, TranslationContext dbContext, string? format = "json") =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));
        
                var language = await GetLanguageAsync(languageCode, dbContext);
                if (language is null)
                    return TypedResults.NotFound(Message.LanguageNotFound(languageCode));
                
                // Check if Translations were created
                var isTranslation = await IsTranslationAsync(env, language, dbContext);
                if (isTranslation is false)
                    return TypedResults.NotFound(Message.TranslationNotFound(envName, languageCode));

                // Check if Keys in dbContext contains all input Translations' keys
                IEnumerable<Key> keys = await GetKeysAsync(env, dbContext);
                IEnumerable<string> keyNames = keys.Select(k => k.Name).ToList();
                IEnumerable<string> updatedTranslationsKeyNames = updatedTranslationDtos.Select(ut => ut.KeyName).ToList();
                IEnumerable<string> differentKeys = updatedTranslationsKeyNames.Except(keyNames);
                if (differentKeys.Any())
                    return TypedResults.BadRequest(Message.KeysNotMatch(string.Join(", ", differentKeys)));

                var duplicates = GetDuplicates(updatedTranslationDtos.Select(ut => ut.KeyName).ToList());
                if (duplicates.Any())
                    return TypedResults.BadRequest(Message.KeyDuplicates(string.Join(", ", duplicates)));

                // Updating the current Translations
                updatedTranslationDtos.ForEach(ut =>
                {
                    var key = keys.FirstOrDefault(k => k.Name == ut.KeyName);
                    var translation = dbContext.Translations.FirstOrDefault(t => t.KeyId == key!.Id && t.LanguageId == language.Id)!;
                    translation.TranslationText = ut.TranslationText;
                    translation.UpdatedAt = DateTime.UtcNow;
                    translation.UpdatedBy = "user.Email";
                });

                await dbContext.SaveChangesAsync();

                return TypedResults.Ok();
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Translation"}],
                OperationId = "PutTranslations",
                Summary = "Update a list of translations in the given language and environment",
                Description = "Request body: List of TranslationNewDto"
            });

        // DELETE /translations/{project}/{environment}/{language}
        group.MapDelete("/{projectName}/{envName}/{languageCode}",
            async Task<Results<NotFound<string>, Ok<int>>>
            (string projectName, string envName, string languageCode, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));
        
                var language = await GetLanguageAsync(languageCode, dbContext);
                if (language is null)
                    return TypedResults.NotFound(Message.LanguageNotFound(languageCode));

                var count =  await DeleteTranslationsAsync(env, language, dbContext);
                if (count is 0)
                    return TypedResults.NotFound(Message.TranslationNotFound(envName, languageCode));
                else
                    return TypedResults.Ok(count);
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Translation"}],
                OperationId = "DeleteTranslations",
                Summary = "Delete all translations in the given language and environment",
                Description = "Response body: Number of deleted translations"
            });

        return group;
    }
}
