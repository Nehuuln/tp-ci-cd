using MongoDB.Bson;
using MongoDB.Driver;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

string host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "27017";
string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "appdb";
string user = Environment.GetEnvironmentVariable("DB_USER") ?? "appuser";
string pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "mypassword";

string mongoUri = $"mongodb://{Uri.EscapeDataString(user)}:{Uri.EscapeDataString(pass)}@{host}:{port}/{dbName}?authSource=admin";
MongoClient client = new MongoClient(mongoUri);
IMongoDatabase db = client.GetDatabase(dbName);
IMongoCollection<BsonDocument> users = db.GetCollection<BsonDocument>("users");

app.MapGet("/health", async () =>
{
    try
    {
        await db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
        return Results.Ok(new { status = "ok", db = "ok" });
    }
    catch
    {
        return Results.StatusCode(503);
    }
});

app.MapPost("/users", async (UserDto body) =>
{
    if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Email))
        return Results.BadRequest(new { error = "name and email are required" });

    BsonDocument doc = new BsonDocument
    {
        { "name", body.Name },
        { "email", body.Email },
        { "createdAt", DateTime.UtcNow }
    };

    await users.InsertOneAsync(doc);
    return Results.Created($"/users/{doc["_id"]}", new { id = doc["_id"].ToString() });
});

app.MapGet("/users", async () =>
{
    List<BsonDocument> list = await users.Find(FilterDefinition<BsonDocument>.Empty).Limit(50).ToListAsync();
    return Results.Ok(list.Select(x => new
    {
        id = x.GetValue("_id").ToString(),
        name = x.GetValue("name", "").AsString,
        email = x.GetValue("email", "").AsString
    }));
});

app.Run("http://0.0.0.0:3000");

record UserDto(string Name, string Email);