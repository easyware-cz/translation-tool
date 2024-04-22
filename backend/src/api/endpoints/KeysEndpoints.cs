using Microsoft.EntityFrameworkCore;
using src.api.dtos;
using src.api.methods;
using static src.api.methods.EndpointsMethods;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class KeysEndpoints
{
    const string KEYROUTE = "GetKey";

    public static RouteGroupBuilder MapKeysEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations/{projectName}/{envName}/keys
        group.MapGet("/{projectName}/{envName}/keys", async (string projectName, string envName, TranslationContext dbContext) =>
        //await dbContext.Keys.Where(k => k.Env!.Project!.Name == projectName && k.Env.Name == envName).Select(key => key.ToDto()).AsNoTracking().ToListAsync()
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));
            
            var keys = await GetKeysAsync(env, dbContext);
            return Results.Ok(keys.Select(k => k.ToDto()).ToList());
        })
            .WithName(KEYROUTE);

        // GET /translations/{projectName}/{envName}/keys/{id}
        group.MapGet("/{projectName}/{envName}/keys/{id}", async (string projectName, string envName, int id, TranslationContext dbContext) =>
        {
            //Key? key = await dbContext.Keys.FirstOrDefaultAsync(k => k.Env!.Project!.Name == projectName && k.Env.Name == envName && k.Id == id);
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));

            var key = await GetKeyAsync(env, id, dbContext);
            if (key is null)
                return Results.NotFound(Message.KeyNotFound(projectName, envName, id));
            else
                return Results.Ok(key.ToDto);
        });

        // POST /translations/{projectName}/{envName}/keys
        group.MapPost("/{projectName}/{envName}/keys", async (string projectName, string envName, List<KeyNewDto> newKeyDtos, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));

            // Input Keys must be unigue
            var newKeys = newKeyDtos.Select(k => k.ToEntity(env.Id)).ToList();
            var duplicates = GetDuplicates(newKeys.Select(k => k.Name));
            if (duplicates.Any()) // 'Count !=0' performs slightly better than 'Any()'
                return Results.BadRequest(Message.KeyDuplicates(string.Join(", ", duplicates)));

            // Input Keys must not match the Keys already entered
            var currentKeys = await GetKeysAsync(env, dbContext);
            duplicates = GetDuplicates(newKeys.Select(k => k.Name).Concat(currentKeys.Select(k => k.Name)));//var sameKeyNames = currentKeys.Select(k => k.Name).Intersect(newKeys.Select(k => k.Name));
            if (duplicates.Any())
                return Results.BadRequest(Message.KeysAlreadyExist(string.Join(", ", duplicates)));

            await dbContext.Keys.AddRangeAsync(newKeys);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(KEYROUTE, new { projectName, envName }, newKeys.Select(k => k.ToDto()).ToList());
        });

        // PUT /translations/{projectName}/{envName}/keys/{id}
        group.MapPut("/{projectName}/{envName}/keys/{id}", async (string projectName, string envName, int id, KeyNewDto updatedKeyDto, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));

            var currentKey = await GetKeyAsync(env, id, dbContext);
            if (currentKey is null)
                return Results.NotFound(Message.KeyNotFound(projectName, envName, id));

            Key updatedKey = updatedKeyDto.ToEntity(currentKey.EnvId, currentKey.Id);
            var sameKey = await GetKeyAsync(env, updatedKey.Name, dbContext);
            if (sameKey is not null && !sameKey.Name.Equals(currentKey.Name)) 
                return Results.BadRequest(Message.KeyDuplicates(sameKey.Name));

            await PutEntityAsync(currentKey, updatedKey, dbContext);

            return Results.Ok();
        });

        // DELETE /translations/{projectName}/{envName}/keys
        group.MapDelete("/{projectName}/{envName}/keys", async (string projectName, string envName, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));

            int count = await DeleteKeysAsync(env, dbContext);
            if (count is 0)
                return Results.NotFound(Message.KeysNotFound(projectName, envName));
            else
                return Results.Ok(count);
        });

        return group;
    }
}
