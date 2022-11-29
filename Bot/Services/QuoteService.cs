using Bot.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Bot.Services;

public class QuoteService
{
    private readonly IMongoCollection<Quote> _quoteCollection;

    public QuoteService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _quoteCollection = mongoDatabase.GetCollection<Quote>("quotesCollection");
    }

    public async Task<List<Quote>> GetAsync() =>
        await _quoteCollection.Find(_ => true).ToListAsync();

    public async Task<Quote?> GetAsync(string id) =>
        await _quoteCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Quote> GetRandom() =>
        await _quoteCollection
            .AggregateAsync<Quote>(new[] { new BsonDocument { { "$sample", new BsonDocument { { "size", 1 } } } } })
            .Result
            .FirstOrDefaultAsync();
    
    public async Task CreateAsync(Quote newQuotes) =>
        await _quoteCollection.InsertOneAsync(newQuotes);

    public async Task UpdateAsync(string id, Quote updatedQuotes) =>
        await _quoteCollection.ReplaceOneAsync(x => x.Id == id, updatedQuotes);

    public async Task RemoveAsync(string id) =>
        await _quoteCollection.DeleteOneAsync(x => x.Id == id); 
}