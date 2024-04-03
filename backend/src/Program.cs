using src.api.dtos;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

 List<TranslationDto> translations = [
    new (
        1,
        "en",
        "Last name",
        new DateTime(),
        "current user"
    ),
    new (
        2,
        "de",
        "Nachname",
        new DateTime(),
        "current user"
    ),
    new (
        3,
        "cs",
        "Příjmení",
        new DateTime(),
        "current user"
    )
];


// %20 means whitespace
const string GetTranslationEndpointName = "GetTranslation";

// GET /translations
app.MapGet("translations", () => translations);

// GET /translations/{id}
app.MapGet("translations/{id}", (int id) => translations.FindAll(translation => translation.Id == id))
    .WithName(GetTranslationEndpointName);

// POST /translations
app.MapPost("translations", (NewTranslationDto newTranslation) =>
{
    TranslationDto translation = new(
        translations.Count + 1,
        newTranslation.Language,
        newTranslation.Translation,
        DateTime.Now,
        "user.Email"
    );
    translations.Add(translation);

    return Results.CreatedAtRoute(GetTranslationEndpointName, new { id = translation.Id}, translation);
});

// PUT /translations/{id}
app.MapPut("translations/{id}", (int id, NewTranslationDto updatedTranslation) => 
{
    var index = translations.FindIndex(translation => translation.Id == id);
    translations[index] = new TranslationDto(
        id,
        updatedTranslation.Language,
        updatedTranslation.Translation,
        DateTime.Now,
        "user.Email"
    );

    return Results.NoContent();
});


// DELETE /translations/{id}
app.MapDelete("translations/{id}", (int id) =>
{
    translations.RemoveAll(translation => translation.Id == id);

    return Results.NoContent();
});

app.Run();
