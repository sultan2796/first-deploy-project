using MongoDB.Driver;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration["Mongo:ConnectionString"]));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(builder.Configuration["Mongo:Database"]);
});

builder.Services.AddSingleton<IMongoCollection<CSharpFirstApi.Models.Todo>>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    return db.GetCollection<CSharpFirstApi.Models.Todo>("todos");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Ping MongoDB on startup and log result
try
{
    var database = app.Services.GetRequiredService<IMongoDatabase>();
    await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
    Console.WriteLine("[Startup] MongoDB connection successful.");
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup] MongoDB connection FAILED: {ex.Message}");
}

// Minimal API CRUD
app.MapGroup("/api/todos").WithTags("Todos").MapTodosApi();

Console.WriteLine("[Startup] Server starting...");
app.Run();

static class TodosApi
{
    public static RouteGroupBuilder MapTodosApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IMongoCollection<CSharpFirstApi.Models.Todo> col) =>
            Results.Ok(await col.Find(_ => true).ToListAsync()));

        group.MapGet("/{id}", async (string id, IMongoCollection<CSharpFirstApi.Models.Todo> col) =>
        {
            var todo = await col.Find(t => t.Id == id).FirstOrDefaultAsync();
            return todo is null ? Results.NotFound() : Results.Ok(todo);
        });

        group.MapPost("/", async (CSharpFirstApi.Models.Todo input, IMongoCollection<CSharpFirstApi.Models.Todo> col) =>
        {
            await col.InsertOneAsync(input);
            return Results.Created($"/api/todos/{input.Id}", input);
        });

        group.MapPut("/{id}", async (string id, CSharpFirstApi.Models.Todo input, IMongoCollection<CSharpFirstApi.Models.Todo> col) =>
        {
            input.Id = id;
            var res = await col.ReplaceOneAsync(t => t.Id == id, input);
            return res.MatchedCount == 0 ? Results.NotFound() : Results.NoContent();
        });

        group.MapDelete("/{id}", async (string id, IMongoCollection<CSharpFirstApi.Models.Todo> col) =>
        {
            var res = await col.DeleteOneAsync(t => t.Id == id);
            return res.DeletedCount == 0 ? Results.NotFound() : Results.NoContent();
        });

        return group;
    }
}