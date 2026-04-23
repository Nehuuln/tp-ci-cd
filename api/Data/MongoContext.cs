using MongoDB.Bson;
using MongoDB.Driver;

namespace TpApi.Data;

public class MongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<BsonDocument> Users { get; }

    public MongoContext()
    {
        string host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "27017";
        string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "appdb";
        string user = Environment.GetEnvironmentVariable("DB_USER") ?? "appuser";
        string pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "mypassword";

        string mongoUri = $"mongodb://{Uri.EscapeDataString(user)}:{Uri.EscapeDataString(pass)}@{host}:{port}/{dbName}?authSource=admin";

        MongoClient client = new MongoClient(mongoUri);
        Database = client.GetDatabase(dbName);
        Users = Database.GetCollection<BsonDocument>("users");
    }

    public async Task<bool> PingAsync()
    {
        try
        {
            await Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
            return true;
        }
        catch
        {
            return false;
        }
    }
}