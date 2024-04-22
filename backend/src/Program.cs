using Microsoft.EntityFrameworkCore;
using src.api.endpoints;
using src.data.db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<TranslationContext>(options => options.UseNpgsql(connectionString));
//builder.Services.AddEntityFrameworkNpgsql().AddDbContext<TranslationContext>(options => options.UseNpgsql(connectionString));
//builder.Services.AddNpgsql<TranslationContext>(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapProjectsEndpointsAsync();
app.MapEnvsEndpoints();
app.MapLanguagesEndpoints();
app.MapKeysEndpoints();
app.MapTranslationsEndpoints();

//await app.MigrateDbAsync(); // Executing migrations when the application starts

app.Run();
