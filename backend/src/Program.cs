using Microsoft.EntityFrameworkCore;
using src.api.endpoints;
using src.data.db;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<TranslationContext>(options => options.UseNpgsql(connectionString)); //builder.Services.AddEntityFrameworkNpgsql().AddDbContext<TranslationContext>(options => options.UseNpgsql(connectionString));
//builder.Services.AddNpgsql<TranslationContext>(connectionString);

var app = builder.Build();

app.MapTranslationsEndpoints();
app.MapLanguagesEndpoints();

await app.MigrateDbAsync(); // Executing migrations when the application starts

app.Run();
