using Bot.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Bot.Services;

public class TicketService
{
    private readonly IMongoCollection<Ticket> _ticketCollection;

    public TicketService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _ticketCollection = mongoDatabase.GetCollection<Ticket>("tickets");
    }

    public async Task<List<Ticket>> GetAsync() =>
        await _ticketCollection.Find(_ => true).ToListAsync();

    public async Task<Ticket?> GetAsync(string id) =>
        await _ticketCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task<Ticket?> GetByChannelIdAsync(ulong? channelId) =>
        await _ticketCollection.Find(x => x.channelId == channelId).FirstOrDefaultAsync();
    
    public async Task CreateAsync(Ticket newTicket) =>
        await _ticketCollection.InsertOneAsync(newTicket);

    public async Task UpdateAsync(string? id, Ticket updatedTicket) =>
        await _ticketCollection.ReplaceOneAsync(x => x.Id == id, updatedTicket);

    public async Task RemoveAsync(string id) =>
        await _ticketCollection.DeleteOneAsync(x => x.Id == id); 
}