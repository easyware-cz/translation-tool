using Microsoft.EntityFrameworkCore;

namespace src.data.db;

public static class DataExtensions
{
    public static async Task MigrateDbAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TranslationContext>();
        await dbContext.Database.MigrateAsync();
    }
}
