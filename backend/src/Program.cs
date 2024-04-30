using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using src.api.endpoints;
using src.data.db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Translation tool",
        Description = "ASP.NET Core Web API for managing translations<br />This tool was created primarily to learn the basics of web application development.<br /><br /><a href=https://github.com/easyware-cz/translation-tool>GitHub repository</a>"
    });
});

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<TranslationContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapProjectsEndpoints();
app.MapEnvsEndpoints();
app.MapLanguagesEndpoints();
app.MapKeysEndpoints();
app.MapTranslationsEndpoints();

//await app.MigrateDbAsync(); // Executing migrations when the application starts

app.Run();
