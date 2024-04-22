using Microsoft.EntityFrameworkCore;
using src.api.dtos;
using src.data.db;
using src.data.entities;
using src.data.mapping;
using static src.api.methods.EndpointsMethods;
using src.api.methods;

namespace src.api.endpoints;

public static class TranslationsEndpoints
{
    const string TRANSLATIONROUTE = "GetTranslation";

    public static RouteGroupBuilder MapTranslationsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations/{project}/{environment}/{language}?format={json | resx}
        group.MapGet("/{projectName}/{envName}/{languageCode}", async (string projectName, string envName, string languageCode, TranslationContext dbContext, string? format = "json") =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));
    
            var language = await GetLanguageAsync(languageCode, dbContext);
            if (language is null)
                return Results.NotFound(Message.LanguageNotFound(languageCode));

            var translations = await GetTranslationsAsync(env, language, dbContext);
            if (translations is null)
                return Results.NotFound(Message.TranslationNotFound(envName, languageCode));
            else
                return Results.Ok(translations.Select(t => t.ToDto()).ToList());
        })
            .WithName(TRANSLATIONROUTE);

        // POST /translations/{project}/{environment}/{languageCode}
        // For each Project/Env/Key create "empty" Translation (translationText = "") in Project/Env/Language
        group.MapPost("/{projectName}/{envName}/{languageCode}", async (string projectName, string envName, string languageCode, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));
    
            var language = await GetLanguageAsync(languageCode, dbContext);
            if (language is null)
                return Results.NotFound(Message.LanguageNotFound(languageCode));

            var keys = await GetKeysAsync(env, dbContext); // Get list of all Keys for given Project and Environment
            if (keys is null)
                return Results.NotFound(Message.KeysNotFound(projectName, envName) );
            
            if (await IsTranslationAsync(env, language, dbContext) is true)
                return Results.BadRequest(Message.TranslationsAlreadyExist(languageCode));
            
            Translation newTranslation;
            List<Translation> newTranslations = []; //var newTranslations = new List<Translation>();
            TranslationNewDto newTranslationDto; // input Dto
            List<TranslationDto> TranslationDtos = []; // output Dtos
            
            foreach (var key in keys)
            {
                newTranslationDto = new(key.Name, ""); // Translation text will be updated by a PUT request
                newTranslation = newTranslationDto.ToEntity(key.Id, language.Id); // Adding UpdatedAt, UpdatedBy
                newTranslations.Add(newTranslation);
                TranslationDtos.Add(newTranslation.ToDto());
            }

            await dbContext.Translations.AddRangeAsync(newTranslations);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(TRANSLATIONROUTE, new { projectName, envName, languageCode }, TranslationDtos);
        });


        // PUT /translations/{project}/{environment}/{language}?format={json / resx}
        // Add or update the translationText in given Translations
        group.MapPut("/{projectName}/{envName}/{languageCode}", async (string projectName, string envName, string languageCode, List<TranslationNewDto> updatedTranslationDtos, TranslationContext dbContext, string? format = "json") =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));
    
            var language = await GetLanguageAsync(languageCode, dbContext);
            if (language is null)
                return Results.NotFound(Message.LanguageNotFound(languageCode));
            
            // Check if Translations were created
            var isTranslation = await IsTranslationAsync(env, language, dbContext);
            if (isTranslation is false)
                return Results.NotFound(Message.TranslationNotFound(envName, languageCode));

            // Check if Keys in dbContext contains all input Translations' keys
            IEnumerable<Key> keys = await GetKeysAsync(env, dbContext);
            IEnumerable<string> keyNames = keys.Select(k => k.Name).ToList();
            IEnumerable<string> updatedTranslationsKeyNames = updatedTranslationDtos.Select(ut => ut.KeyName).ToList();
            IEnumerable<string> differentKeys = updatedTranslationsKeyNames.Except(keyNames);
            if (differentKeys.Any())
                return Results.BadRequest(Message.KeysNotMatch(string.Join(", ", differentKeys)));

            // Updating the current translations
            updatedTranslationDtos.ForEach(ut =>
            {
                var key = keys.FirstOrDefault(k => k.Name == ut.KeyName);
                var translation = dbContext.Translations.FirstOrDefault(t => t.KeyId == key!.Id && t.LanguageId == language.Id)!;
                translation.TranslationText = ut.TranslationText;
                translation.UpdatedAt = DateTime.UtcNow;
                translation.UpdatedBy = "user.Email";
            });

            await dbContext.SaveChangesAsync();

            return Results.Ok();
        });

        // DELETE /translations/{project}/{environment}/{language}
        group.MapDelete("/{projectName}/{envName}/{languageCode}", async (string projectName, string envName, string languageCode, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));
    
            var language = await GetLanguageAsync(languageCode, dbContext);
            if (language is null)
                return Results.NotFound(Message.LanguageNotFound(languageCode));

            var count =  await DeleteTranslationsAsync(env, language, dbContext);
            if (count is 0)
                return Results.NotFound(Message.TranslationNotFound(envName, languageCode));
            else
                return Results.Ok(count);
        });

        return group;
    }
}
