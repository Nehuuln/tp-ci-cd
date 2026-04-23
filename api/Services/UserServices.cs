using MongoDB.Bson;
using MongoDB.Driver;
using TpApi.Data;

namespace TpApi.Services;

public class UserService
{
    private readonly IMongoCollection<BsonDocument> _users;

    public UserService(MongoContext context)
    {
        _users = context.Users;
    }

    public async Task<string> CreateAsync(string name, string email)
    {
        BsonDocument doc = new()
        {
            { "name", name },
            { "email", email },
            { "createdAt", DateTime.UtcNow }
        };

        await _users.InsertOneAsync(doc);
        return doc["_id"].ToString();
    }

    public async Task<IEnumerable<object>> GetAllAsync()
    {
        List<BsonDocument> list = await _users
            .Find(FilterDefinition<BsonDocument>.Empty)
            .Limit(50)
            .ToListAsync();

        return list.Select(x => new
        {
            id = x.GetValue("_id").ToString(),
            name = x.GetValue("name", "").AsString,
            email = x.GetValue("email", "").AsString
        });
    }
}