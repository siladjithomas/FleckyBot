using Bot.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Bot.Services;

public class VoteService
{
    private readonly IMongoCollection<Vote> _voteCollection;

    public VoteService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _voteCollection = mongoDatabase.GetCollection<Vote>("votes");
    }

    public async Task<List<Vote>> GetAsync() =>
        await _voteCollection.Find(_ => true).ToListAsync();

    public async Task<Vote?> GetAsync(string id) =>
        await _voteCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task<Vote?> GetByMessageIdAsync(ulong? messageId) =>
        await _voteCollection.Find(x => x.messageId == messageId).FirstOrDefaultAsync();
    
    public async Task CreateAsync(Vote newVote) =>
        await _voteCollection.InsertOneAsync(newVote);

    public async Task UpdateAsync(string? id, Vote updatedVote) =>
        await _voteCollection.ReplaceOneAsync(x => x.Id == id, updatedVote);

    public async Task RemoveAsync(string id) =>
        await _voteCollection.DeleteOneAsync(x => x.Id == id); 
}