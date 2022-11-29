using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bot.Models;

public class Ticket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set;}
    public ulong userId { get; set; } = 0;
    public string userName { get; set; } = string.Empty;
    public ulong channelId { get; set; } = 0;
    public string channelName { get; set; } = string.Empty;
    public bool isClosed { get; set; } = false;
    public List<Message> messages { get; set; } = new List<Message>();
}

public class Message
{
    public ulong messageId { get; set; } = 0;
    public ulong userId { get; set; } = 0;
    public string userName { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
}