using Bot.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Bot.Services;

public class GuildService
{
    private readonly IMongoCollection<Guild> _guildCollection;

    public GuildService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _guildCollection = mongoDatabase.GetCollection<Guild>("guilds");
    }

    public async Task<List<Guild>> GetAsync() =>
        await _guildCollection.Find(_ => true).ToListAsync();

    public async Task<Guild> GetAsync(string id) =>
        await _guildCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Guild> GetByGuildIdAsync(ulong? guildId) =>
        await _guildCollection.Find(x => x.GuildId == guildId).FirstOrDefaultAsync();
    
    public async Task<List<Guild>> GetGuildsByAdminId(ulong adminId) =>
        await _guildCollection.Find(x => x.GuildAdminId == adminId).ToListAsync();

    public async Task CreateAsync(Guild newGuild) =>
        await _guildCollection.InsertOneAsync(newGuild);

    public async Task UpdateAsync(string id, Guild updatedGuild) =>
        await _guildCollection.ReplaceOneAsync(x => x.Id == id, updatedGuild);

    public async Task RemoveAsync(string id) =>
        await _guildCollection.DeleteOneAsync(x => x.Id == id);
}