using src.api.dtos;
using src.api.methods;
using static src.api.methods.EndpointsMethods;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class EnvsEndpoints
{
    const string ENVROUTE = "GetEnv";
    public const string PROJECTROUTE = "GetProject";    

    public static RouteGroupBuilder MapEnvsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations/{projectName}
        group.MapGet("/{projectName}/", async (string projectName, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                 return Results.NotFound(Message.ProjectNotFound(projectName));
            
            var envs = await GetEnvsAsync(project, dbContext);
            return Results.Ok(envs.Select(e => e.ToDto()).ToList());
        })
            .WithName(PROJECTROUTE);

        // GET /translations/{projectName}/{envName}
        group.MapGet("/{projectName}/{envName}", async (string projectName, string envName, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            var env = await GetEnvAsync(project, envName, dbContext);
            if (env is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));
            else
                return Results.Ok(env.ToDto());
        })
            .WithName(ENVROUTE);

        // POST /translations/{projectName}
        group.MapPost("/{projectName}", async (string projectName, EnvNewDto newEnvDto, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));
            
            Env newEnv = newEnvDto.ToEntity(project.Id);

            var sameEnv = await GetEnvAsync(project, newEnv.Name, dbContext);
            if (sameEnv is not null)
                return Results.BadRequest(Message.EnvAlreadyExists(projectName, newEnv.Name));

            await dbContext.Envs.AddAsync(newEnv);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(ENVROUTE, new { projectName, envName = newEnv.Name }, newEnv.ToDto());
        });

        // PUT /translations/{projectName}/{envName}
        group.MapPut("/{projectName}/{envName}", async (string projectName, string envName, EnvNewDto updatedEnvDto, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));
            
            var currentEnv = await GetEnvAsync(project, envName, dbContext);
            if (currentEnv is null)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));

            Env updatedEnv = updatedEnvDto.ToEntity(currentEnv.ProjectId, currentEnv.Id);

            var sameEnv = await GetEnvAsync(project, updatedEnv.Name, dbContext);
            if (sameEnv is not null)
                return Results.BadRequest(Message.EnvAlreadyExists(projectName, updatedEnv.Name));

            await PutEntityAsync(currentEnv, updatedEnv, dbContext);
            
            return Results.Ok();
        });

        // DELETE /translations/{projectName}/{envName}
        group.MapDelete("/{projectName}/{envName}", async (string projectName, string envName, TranslationContext dbContext) =>
        {
            var project = await GetProjectAsync(projectName, dbContext);
            if (project is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));

            int count = await DeleteEnvAsync(project, envName, dbContext);
            if (count is 0)
                return Results.NotFound(Message.EnvNotFound(projectName, envName));
            else
                return Results.Ok();
        });

        return group;
    }
}
