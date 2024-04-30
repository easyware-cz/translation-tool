using Microsoft.AspNetCore.Http.HttpResults;
using src.api.dtos;
using src.api.methods;
using static src.api.utilities.EndpointsMethods;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.endpoints;

public static class ProjectsEndpoints
{
    public static RouteGroupBuilder MapProjectsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("translations").WithParameterValidation();

        // GET /translations
        group.MapGet("/",
            async Task<Ok<List<ProjectDto>>>
            (TranslationContext dbContext) => 
            {
                var projects = await GetProjectsAsync(dbContext);
                //return TypedResults.Ok();
                return TypedResults.Ok(projects.Select(p => p.ToDto()).ToList());
            })
            // excludes endpoint from generating an OpenAPI description:
            //.ExcludeFromDescription();
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Project"}], // Tags = new List<OpenApiTag> { new() {Name = "Projects"}},
                OperationId = "GetProjects",
                Summary = "Get a list of all projects",
                Description = "Response body: List of ProjectDto"
            });
            // .WithOpenApi(operation =>
            // {
            //     var parameter1 = operation.Parameters[0];
            //     parameter1.Description = "New project DTO";
            //     return operation;
            // });

        // POST /translations
        group.MapPost("/",
            async Task<Results<BadRequest<string>, CreatedAtRoute<ProjectDto>>>
            (ProjectNewDto newProjectDto, TranslationContext dbContext) =>
            {
                Project newProject = newProjectDto.ToEntity();
                
                var sameProject = await GetProjectAsync(newProject.Name, dbContext);
                if (sameProject is not null)
                    return TypedResults.BadRequest(Message.ProjectAlreadyExists(newProject.Name));
                
                await dbContext.Projects.AddAsync(newProject);
                await dbContext.SaveChangesAsync();

                return TypedResults.CreatedAtRoute(
                    routeName: EnvsEndpoints.PROJECTROUTE,
                    routeValues: new { projectName = newProject.Name },
                    value: newProject.ToDto());
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Project"}],
                OperationId = "PostProject",
                Summary = "Add a new project",
                Description = "Request body: ProjectNewDto<br />Response body: ProjectDto"
            });

        // PUT /translations/{projectName}
        group.MapPut("/{projectName}",
            async Task<Results<NotFound<string>, BadRequest<string>, Ok>>
            (string projectName, ProjectNewDto updatedProjectDto, TranslationContext dbContext) =>
            {
                var currentProject = await GetProjectAsync(projectName, dbContext);
                if (currentProject is null)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));
                
                Project updatedProject = updatedProjectDto.ToEntity(currentProject.Id);
                
                var sameProject = await GetProjectAsync(updatedProject.Name, dbContext);
                if (sameProject is not null)
                    return TypedResults.BadRequest(Message.ProjectAlreadyExists(updatedProject.Name));

                await PutEntityAsync(currentProject, updatedProject, dbContext);

                return TypedResults.Ok();
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Project"}],
                OperationId = "PutProject",
                Summary = "Update the project",
                Description = "Request body: ProjectNewDto"
            });

        // DELETE /translations/{projectName}
        group.MapDelete("/{projectName}",
            async Task<Results<NotFound<string>, Ok<int>>>
            (string projectName, TranslationContext dbContext) =>
            {
                int count = await DeleteProjectAsync(projectName, dbContext);
                if (count is 0)
                    return TypedResults.NotFound(Message.ProjectNotFound(projectName));
                else
                    return TypedResults.Ok(count);
            })
            .WithOpenApi(operation => new(operation)
            {
                Tags = [new() {Name = "Project"}],
                OperationId = "DeleteProject",
                Summary = "Dekete the project",
                Description = ""
            });

        return group;
    }
}