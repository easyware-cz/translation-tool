using Microsoft.EntityFrameworkCore;
using src.api.dtos;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class TranslationsEndpoints
{
    const string TranslationRoute = "GetTranslation";

    private static string MessageProjectEnvNotFound(string projectName, string envName)
    {
        return $"Project {projectName} with environment {envName} was not found.";
    }
    private static string MessageLanguageNotFound(string languageCode)
    {
        return $"Language code {languageCode} was not found.";
    }

    private static string MessageTranslationNotFound(string envName, string languageCode)
    {
        return $"Environment {envName} with the language code {languageCode} was not found.";
    }

    private static string MessageTranslationsAlreadyExist(string envName, string languageCode)
    {
        return $"Tranlations with the language code {languageCode} already exist.";
    }

    private static async Task<int> GetEnvId(string projectName, string envName, TranslationContext dbContext)
    {
        Env? env = await dbContext.Envs.FirstOrDefaultAsync(e => e.Project!.Name == projectName && e.Name == envName);
        return (env is null) ? -1 : env.Id;
    }

    private static async Task<int> GetLanguageId(string languageCode, TranslationContext dbContext)
    {
        Language? language = await dbContext.Languages.FirstOrDefaultAsync(l => l.LanguageCode == languageCode);
        return (language is null) ? -1 : language.Id;
    }

    private static async Task<List<Translation>> GetTranslations(int envId, int languageId, TranslationContext dbContext)
    {
        return await dbContext.Translations.Where(t => t.Key!.EnvId == envId && t.LanguageId == languageId).ToListAsync();
    }

    private static async Task<bool> IsTranslation(int envId, int languageId, TranslationContext dbContext)
    {
        return await dbContext.Translations.FirstOrDefaultAsync(t => t.Key!.EnvId == envId && t.LanguageId == languageId) is Translation translation;
    }

    public static RouteGroupBuilder MapTranslationsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations/{project}/{environment}/{language}?format={json | resx}
        group.MapGet("/{projectName}/{envName}/{languageCode}", async (string projectName, string envName, string languageCode, TranslationContext dbContext, string? format = "json") =>
        {
            int envId = await GetEnvId(projectName, envName, dbContext);
            if (envId is -1) return Results.BadRequest(MessageProjectEnvNotFound(projectName, envName));

            int languageId = await GetLanguageId(languageCode, dbContext);
            if (languageId is -1) return Results.BadRequest(MessageLanguageNotFound(languageCode));

            if (await IsTranslation(envId, languageId, dbContext) is false)
                return Results.BadRequest(MessageTranslationNotFound(envName, languageCode));

            List<TranslationDto> translationDtos = await dbContext.Translations
                .Where(t => t.Key!.EnvId == envId && t.LanguageId == languageId)
                //.Include(t => t.Key!.Name)
                //.Include(t => t.Key!.Description)
                //.Include(t => t.Language!.LanguageCode)
                //.Select(t => t.ToFullDto())
                .Select(t => t.ToDto())
                .ToListAsync();

            return translationDtos.Any() ? Results.Ok(translationDtos) : Results.NotFound();
        })
            .WithName(TranslationRoute);

        // POST /translations/{project}/{environment}/{languageCode}
        group.MapPost("/{projectName}/{envName}/{languageCode}", async (string projectName, string envName, string languageCode, TranslationContext dbContext) =>
        {
            int envId = await GetEnvId(projectName, envName, dbContext);
            if (envId is -1) return Results.BadRequest(MessageProjectEnvNotFound(projectName, envName));

            int languageId = await GetLanguageId(languageCode, dbContext);
            if (languageId is -1) return Results.BadRequest(MessageLanguageNotFound(languageCode));

            if (await IsTranslation(envId, languageId, dbContext) is true)
                return Results.BadRequest(MessageTranslationsAlreadyExist(envName, languageCode));
            
            // list of all Keys for given Project and Environment  
            List<Key> keys = await dbContext.Keys.Where(k => k.EnvId == envId).ToListAsync();

            TranslationNewDto newTranslationDto;
            List<TranslationNewDto> newTranslationDtos = [];
            List<Translation> newTranslations = []; //var newTranslations = new List<Translation>();
            keys.ForEach(k =>
            {
                newTranslationDto = new(k.Name, ""); // Translation text will be updated by a PUT request
                newTranslationDtos.Add(newTranslationDto);
                newTranslations.Add(newTranslationDto.ToEntity(k.Id, languageId));
            });

            await dbContext.Translations.AddRangeAsync(newTranslations);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(TranslationRoute, new { projectName, envName, languageCode }, newTranslationDtos);
        });


        // PUT /translations/{project}/{environment}/{language}?format={json / resx}
        group.MapPut("/{projectName}/{envName}/{languageCode}", async (string projectName, string envName, string languageCode, List<TranslationNewDto> updatedTranslationDtos, TranslationContext dbContext, string? format = "json") =>
        {
            int envId = await GetEnvId(projectName, envName, dbContext);
            if (envId is -1) return Results.BadRequest(MessageProjectEnvNotFound(projectName, envName));
            int languageId = await GetLanguageId(languageCode, dbContext);
            if (languageId is -1) return Results.BadRequest(MessageLanguageNotFound(languageCode));
            Translation? translation = await dbContext.Translations.FirstOrDefaultAsync(t => t.Key!.EnvId == envId && t.LanguageId == languageId);
            if (translation is null) return Results.BadRequest($"Environment {envName} with the language code {languageCode} was not found.");

            // list of all Keys for given Project and Environment  
            IEnumerable<Key> keys = await dbContext.Keys.Where(k => k.EnvId == envId).ToListAsync();
            // check if the Keys in dbContext contains all input Translations' keys
            IEnumerable<string> keyNames = keys.Select(k => k.Name).ToList();
            IEnumerable<string> updatedTranslationsKeyNames = updatedTranslationDtos.Select(ut => ut.KeyName).ToList();
            IEnumerable<string> differentKeys = updatedTranslationsKeyNames.Except(keyNames);

            if (differentKeys.Any()) return Results.BadRequest($"Input translations' keys ({string.Join(", ", differentKeys)}) do not match the resource keys.");
            //newTranslationsKeysNames.Except(keysNames).Any();
            //newTranslations.All(nt.KeyId => keys.Any(key.Id => key.Id == nt.KeyId));

            // updating the current translations
            updatedTranslationDtos.ForEach(ut =>
            {
                var key = keys.FirstOrDefault(k => k.Name == ut.KeyName);
                var translation = dbContext.Translations.FirstOrDefault(t => t.KeyId == key!.Id && t.LanguageId == languageId)!;
                translation.TranslationText = ut.TranslationText;
                translation.UpdatedAt = DateTime.UtcNow;
                translation.UpdatedBy = "user.Email";
            });

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /translations/{project}/{environment}/{language}
        group.MapDelete("/{projectName}/{envName}/{languageCode}", async (string projectName, string envName, string languageCode, TranslationContext dbContext) =>
        {
            int envId = await GetEnvId(projectName, envName, dbContext);
            if (envId is -1) return Results.BadRequest(MessageProjectEnvNotFound(projectName, envName));
            int languageId = await GetLanguageId(languageCode, dbContext);
            if (languageId is -1) return Results.BadRequest(MessageLanguageNotFound(languageCode));

            await dbContext.Translations.Where(t => t.Key!.EnvId == envId && t.LanguageId == languageId).ExecuteDeleteAsync();
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }
}
