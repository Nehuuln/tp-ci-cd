using TpApi.Data;
using TpApi.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<UserService>();

WebApplication app = builder.Build();

app.MapGet("/health", async (MongoContext ctx) =>
{
    bool ok = await ctx.PingAsync();
    return ok ? Results.Ok(new { status = "ok", db = "ok" }) : Results.StatusCode(503);
});

app.MapPost("/users", async (UserDto body, UserService service) =>
{
    if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Email))
        return Results.BadRequest(new { error = "name and email are required" });

    string id = await service.CreateAsync(body.Name, body.Email);
    return Results.Created($"/users/{id}", new { id });
});

app.MapGet("/users", async (UserService service) =>
{
    IEnumerable<object> users = await service.GetAllAsync();
    return Results.Ok(users);
});

app.Run("http://0.0.0.0:3000");

record UserDto(string Name, string Email);