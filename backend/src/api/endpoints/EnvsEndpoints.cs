using Microsoft.AspNetCore.Http.HttpResults;
using src.api.dtos;
using src.api.methods;
using static src.api.utilities.EndpointsMethods;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class EnvsEndpoints
{
    public const string PROJECTROUTE = "GetProject";
    const string ENVROUTE = "GetEnv";

    public static RouteGroupBuilder MapEnvsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations/{projectName}
        group.MapGet("/{projectName}/",
            async Task<Results<NotFound<string>, Ok<List<EnvDto>>>>
            (string projectName, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));
                
                var envs = await GetEnvsAsync(project, dbContext);
                return TypedResults.Ok(envs.Select(e => e.ToDto()).ToList());
            })
            .WithName(PROJECTROUTE)
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Environment"}],
                OperationId = "GetEnvs",
                Summary = "Get a list of all environments in the project",
                Description = "Response body: List of EnvDto"
            });

        // GET /translations/{projectName}/{envName}
        group.MapGet("/{projectName}/{envName}",
            async Task<Results<NotFound<string>, Ok<EnvFullDto>>>
            (string projectName, string envName, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                var env = await GetEnvAsync(project, envName, dbContext);
                if (env is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));
                else
                {
                    var localizations = dbContext.Translations.Where(t => t.Key!.EnvId == env!.Id).Select(t => t.Language!.LanguageCode).ToHashSet();
                    //var localizations = new HashSet<string>(languageCodes);
                    return TypedResults.Ok(env.ToDto(localizations));
                }
            })
            .WithName(ENVROUTE)
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Environment"}],
                OperationId = "GetEnv",
                Summary = "Get the environment with a list of localizations",
                Description = "Response body: EnvFullDto"
            });

        // POST /translations/{projectName}
        group.MapPost("/{projectName}",
            async Task<Results<NotFound<string>, BadRequest<string>, CreatedAtRoute<EnvDto>>>
            (string projectName, EnvNewDto newEnvDto, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));
                
                Env newEnv = newEnvDto.ToEntity(project.Id);

                var sameEnv = await GetEnvAsync(project, newEnv.Name, dbContext);
                if (sameEnv is not null)
                    return TypedResults.BadRequest(Message.EnvAlreadyExists(projectName, newEnv.Name));

                await dbContext.Envs.AddAsync(newEnv);
                await dbContext.SaveChangesAsync();

                return TypedResults.CreatedAtRoute(
                    routeName: ENVROUTE,
                    routeValues: new { projectName, envName = newEnv.Name },
                    value: newEnv.ToDto());
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Environment"}],
                OperationId = "PostEnv",
                Summary = "Add a new environment to the project",
                Description = "Request body: EnvNewDto<br />Response body: EnvDto"
            });

        // PUT /translations/{projectName}/{envName}
        group.MapPut("/{projectName}/{envName}",
            async Task<Results<NotFound<string>, BadRequest<string>, Ok>>
            (string projectName, string envName, EnvNewDto updatedEnvDto, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));
                
                var currentEnv = await GetEnvAsync(project, envName, dbContext);
                if (currentEnv is null)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));

                Env updatedEnv = updatedEnvDto.ToEntity(currentEnv.ProjectId, currentEnv.Id);

                var sameEnv = await GetEnvAsync(project, updatedEnv.Name, dbContext);
                if (sameEnv is not null)
                    return TypedResults.BadRequest(Message.EnvAlreadyExists(projectName, updatedEnv.Name));

                await PutEntityAsync(currentEnv, updatedEnv, dbContext);
                
                return TypedResults.Ok();
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Environment"}],
                OperationId = "PutEnv",
                Summary = "Update the environment in the project",
                Description = "Request body: EnvNewDto"
            });

        // DELETE /translations/{projectName}/{envName}
        group.MapDelete("/{projectName}/{envName}",
            async Task<Results<NotFound<string>, Ok>>
            (string projectName, string envName, TranslationContext dbContext) =>
            {
                var project = await GetProjectAsync(projectName, dbContext);
                if (project is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));

                int count = await DeleteEnvAsync(project, envName, dbContext);
                if (count is 0)
                    return TypedResults.NotFound(Message.EnvNotFound(projectName, envName));
                else
                    return TypedResults.Ok();
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Environment"}],
                OperationId = "DeleteEnv",
                Summary = "Delete the environment in the project",
                Description = ""
            });

        return group;
    }
}
