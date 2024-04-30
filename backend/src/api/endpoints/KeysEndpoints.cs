using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using src.api.dtos;
using src.api.methods;
using static src.api.utilities.EndpointsMethods;
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
        group.MapGet("/{projectName}/{envName}/keys",
            async Task<Results<NotFound<string>, Ok<List<KeyDto>>>>
            (string projectName, string envName, TranslationContext dbContext) =>
            //await dbContext.Keys.Where(k => k.Env!.Project!.Name == projectName && k.Env.Name == envName).Select(key => key.ToDto()).AsNoTracking().ToListAsync()
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));
                
                var keys = await GetKeysAsync(env, dbContext);
                return TypedResults.Ok(keys.Select(k => k.ToDto()).ToList());
            })
            .WithName(KEYROUTE)
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Resource key"}],
                OperationId = "GetKeys",
                Summary = "Get a list of all resource keys in the environment",
                Description = "Response body: List of KeyDto"
            });

        // GET /translations/{projectName}/{envName}/keys/{id}
        group.MapGet("/{projectName}/{envName}/keys/{id}",
            async Task<Results<NotFound<string>, Ok<KeyDto>>>
            (string projectName, string envName, int id, TranslationContext dbContext) =>
            {
                //Key? key = await dbContext.Keys.FirstOrDefaultAsync(k => k.Env!.Project!.Name == projectName && k.Env.Name == envName && k.Id == id);
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));

                var key = await GetKeyAsync(env, id, dbContext);
                if (key is null)
                    return TypedResults.NotFound(Message.KeyNotFound(projectName, envName, id));
                else
                    return TypedResults.Ok(key.ToDto());
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Resource key"}],
                OperationId = "GetKey",
                Summary = "Get the key in the environment",
                Description = "Response body: KeyDto"
            });

        // POST /translations/{projectName}/{envName}/keys
        group.MapPost("/{projectName}/{envName}/keys",
            async Task<Results<NotFound<string>, BadRequest<string>, CreatedAtRoute<List<KeyDto>>>>
            (string projectName, string envName, List<KeyNewDto> newKeyDtos, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));

                // Input Keys must be unigue
                var newKeys = newKeyDtos.Select(k => k.ToEntity(env.Id)).ToList();
                var duplicates = GetDuplicates(newKeys.Select(k => k.Name));
                if (duplicates.Any()) // 'Count !=0' performs slightly better than 'Any()'
                    return TypedResults.BadRequest(Message.KeyDuplicates(string.Join(", ", duplicates)));

                // Input Keys must not match the Keys already entered
                var currentKeys = await GetKeysAsync(env, dbContext);
                duplicates = GetDuplicates(newKeys.Select(k => k.Name).Concat(currentKeys.Select(k => k.Name)));//var sameKeyNames = currentKeys.Select(k => k.Name).Intersect(newKeys.Select(k => k.Name));
                if (duplicates.Any())
                    return TypedResults.BadRequest(Message.KeysAlreadyExist(string.Join(", ", duplicates)));

                await dbContext.Keys.AddRangeAsync(newKeys);
                await dbContext.SaveChangesAsync();

                return TypedResults.CreatedAtRoute(
                    routeName: KEYROUTE,
                    routeValues: new { projectName, envName },
                    value: newKeys.Select(k => k.ToDto()).ToList());
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Resource key"}],
                OperationId = "PostKeys",
                Summary = "Add a list of resource keys to the environment",
                Description = "Request body: List of KeyNewDto<br />Response body: List of KeyDto"
            });

        // PUT /translations/{projectName}/{envName}/keys/{id}
        group.MapPut("/{projectName}/{envName}/keys/{id}",
            async Task<Results<NotFound<string>, BadRequest<string>, Ok>>
            (string projectName, string envName, int id, KeyNewDto updatedKeyDto, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));

                var currentKey = await GetKeyAsync(env, id, dbContext);
                if (currentKey is null)
                    return TypedResults.NotFound(Message.KeyNotFound(projectName, envName, id));

                Key updatedKey = updatedKeyDto.ToEntity(currentKey.EnvId, currentKey.Id);
                var sameKey = await GetKeyAsync(env, updatedKey.Name, dbContext);
                if (sameKey is not null && !sameKey.Name.Equals(currentKey.Name)) 
                    return TypedResults.BadRequest(Message.KeyDuplicates(sameKey.Name));

                await PutEntityAsync(currentKey, updatedKey, dbContext);

                return TypedResults.Ok();
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Resource key"}],
                OperationId = "PutKey",
                Summary = "Update the resource key in the environment",
                Description = "Request body: KeyNewDto"
            });

        // DELETE /translations/{projectName}/{envName}/keys
        group.MapDelete("/{projectName}/{envName}/keys",
            async Task<Results<NotFound<string>, Ok<int>>>
            (string projectName, string envName, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));

                int count = await DeleteKeysAsync(env, dbContext);
                if (count is 0)
                    return TypedResults.NotFound(Message.KeysNotFound(projectName, envName));
                else
                    return TypedResults.Ok(count);
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Resource key"}],
                OperationId = "DeleteKeys",
                Summary = "Delete all resource keys in the environment",
                Description = "Response: number of deleted resource keys"
            });

        return group;
    }
}
