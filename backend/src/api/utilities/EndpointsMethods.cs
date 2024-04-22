using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using src.api.dtos;
using src.data.db;
using src.data.entities;
using src.data.mapping;

namespace src.api.methods;

public static class EndpointsMethods
{
    // // Source https://stackoverflow.com/questions/40495439/c-sharp-generics-for-dbsetsomething?rq=3
    // // Call method:
    // await GetEntityAsync<Project>(projectName, dbContext.Projects)
    
    // // Method without entity interface
    // public static async Task<T> GetEntityAsync<T>(string name, DbSet<T> dbSet) where T : class
    // {
    //     var parameter = Expression.Parameter(typeof(T), "entity");
    //     // generate expression entity => entity.Name = name
    //     var condition = (Expression<Func<T, bool>>)
    //         Expression.Lambda(
    //             Expression.Equal(
    //                 Expression.Property(parameter, "Name"),
    //                 Expression.Constant(name)
    //             )
    //         , parameter);
    //     return await dbSet.FirstOrDefaultAsync(condition);
    // }

    // // Method with entity interface
    // public static async Task<T> GetEntityAsync<T>(string name, DbSet<T> dbSet) where T : class, IHasName
    // {
    //     return await dbSet.SingleOrDefaultAsync(p => p.Name == name);
    // }

    // // Interface
    // public interface IHasName
    // {
    //     string Name { get; set; }
    // }
    // public class Project : IHasName

    public static List<T> GetDuplicates<T>(IEnumerable<T> list)
    {
        HashSet<T> set = [];
        List<T> duplicates = [];
        foreach (T item in list)
        {
            if (!set.Add(item))
                duplicates.Add(item);
        }
        return duplicates;
    }
    
    public static async Task PutEntityAsync(object currentEntity, object updatedEntity, TranslationContext dbContext)
    {
        dbContext.Entry(currentEntity).CurrentValues.SetValues(updatedEntity);
        await dbContext.SaveChangesAsync();
    }
    
    // public static async Task<int> GetProjectId(string projectName, TranslationContext dbContext)
    // {
    //     Project? project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
    //     return project is null ? -1 : project.Id;
    // }
    public static async Task<Project?> GetProjectAsync(string projectName, TranslationContext dbContext)
    {
        return await dbContext.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
    }

    public static async Task<List<Project>> GetProjectsAsync(TranslationContext dbContext)
    {
        return await dbContext.Projects.AsNoTracking().ToListAsync();
    }

    public static async Task<int> DeleteProjectAsync(string projectName, TranslationContext dbContext)
    {
        return await dbContext.Projects.Where(p => p.Name == projectName).ExecuteDeleteAsync();
    }

    public static async Task<Env?> GetEnvAsync(Project project, string envName, TranslationContext dbContext)
    {
        return await dbContext.Envs.FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Name == envName);
    }

    public static async Task<List<Env>> GetEnvsAsync(Project project, TranslationContext dbContext)
    {
        return await dbContext.Envs.Where(e => e.ProjectId == project.Id).AsNoTracking().ToListAsync();
    }

    public static async Task<int> DeleteEnvAsync(Project project, string envName, TranslationContext dbContext)
    {
        return await dbContext.Envs.Where(e => e.ProjectId == project.Id && e.Name == envName).ExecuteDeleteAsync();
    }

    public static async Task<Key?> GetKeyAsync(Env env, int id, TranslationContext dbContext)
    {
        return await dbContext.Keys.FirstOrDefaultAsync(k => k.EnvId == env.Id && k.Id == id);
    }
    
    public static async Task<Key?> GetKeyAsync(Env env, string keyName, TranslationContext dbContext)
    {
        return await dbContext.Keys.FirstOrDefaultAsync(k => k.EnvId == env.Id && k.Name == keyName);
    }
    
    public static async Task<List<Key>> GetKeysAsync(Env env, TranslationContext dbContext)
    {
        return await dbContext.Keys.Where(k => k.EnvId == env.Id).AsNoTracking().ToListAsync();;
    }
    
    public static async Task<int> DeleteKeysAsync(Env env, TranslationContext dbContext)
    {
        return await dbContext.Keys.Where(k => k.EnvId == env.Id).ExecuteDeleteAsync();
    }

    public static async Task<Language?> GetLanguageAsync(string languageCode, TranslationContext dbContext)
    {
        return await dbContext.Languages.FirstOrDefaultAsync(l => l.LanguageCode == languageCode);
    }

    public static async Task<List<Language>> GetLanguagesAsync(TranslationContext dbContext)
    {
        return await dbContext.Languages.AsNoTracking().ToListAsync();
    }

    public static async Task<List<Translation>> GetTranslationsAsync(Env env, Language language, TranslationContext dbContext)
    {
        return await dbContext.Translations.Where(t => t.Key!.EnvId == env.Id && t.LanguageId == language.Id)
            .Include(t => t.Key)
            .Include(t => t.Language)
            .AsNoTracking()
            .ToListAsync();
    }

    public static async Task<int> DeleteTranslationsAsync(Env env, Language language, TranslationContext dbContext)
    {
        return await dbContext.Translations.Where(t => t.Key!.EnvId == env.Id && t.LanguageId == language.Id).ExecuteDeleteAsync();
    }

    public static async Task<bool> IsTranslationAsync(Env env, Language language, TranslationContext dbContext)
    {
        return await dbContext.Translations.FirstOrDefaultAsync(t => t.Key!.EnvId == env.Id && t.LanguageId == language.Id) is Translation translation;
    }
}
