using src.api.dtos;
using src.api.methods;
using static src.api.methods.EndpointsMethods;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class ProjectsEndpoints
{
    public static async Task<RouteGroupBuilder> MapProjectsEndpointsAsync(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations
        group.MapGet("/", async (TranslationContext dbContext) => 
        {
            var projects = await GetProjectsAsync(dbContext);
            return Results.Ok(projects.Select(p => p.ToDto()).ToList());
        });

        // POST /translations
        group.MapPost("/", async (ProjectNewDto newProjectDto, TranslationContext dbContext) =>
        {
            Project newProject = newProjectDto.ToEntity();
            
            var sameProject = await GetProjectAsync(newProject.Name, dbContext);
            if (sameProject is not null)
                return Results.BadRequest(Message.ProjectAlreadyExists(newProject.Name));
            
            await dbContext.Projects.AddAsync(newProject);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(EnvsEndpoints.PROJECTROUTE, new { projectName = newProject.Name }, newProject.ToDto());
        });

        // PUT /translations/{projectName}
        group.MapPut("/{projectName}", async (string projectName, ProjectNewDto updatedProjectDto, TranslationContext dbContext) =>
        {
            var currentProject = await GetProjectAsync(projectName, dbContext);
            if (currentProject is null)
                return Results.NotFound(Message.ProjectNotFound(projectName));
            
            Project updatedProject = updatedProjectDto.ToEntity(currentProject.Id);
            
            var sameProject = await GetProjectAsync(updatedProject.Name, dbContext);
            if (sameProject is not null)
                return Results.BadRequest(Message.ProjectAlreadyExists(updatedProject.Name));

            await PutEntityAsync(currentProject, updatedProject, dbContext);

            return Results.Ok();
        });

        // DELETE /translations/{projectName}
        group.MapDelete("/{projectName}", async (string projectName, TranslationContext dbContext) =>
        {
            int count = await DeleteProjectAsync(projectName, dbContext);
            if (count is 0)
                return Results.NotFound(Message.ProjectNotFound(projectName));
            else
                return Results.Ok(count);
        });

        return group;
    }
}