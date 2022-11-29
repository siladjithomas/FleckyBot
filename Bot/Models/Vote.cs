using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bot.Models;

public class Vote
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public ulong messageId { get; set; }
    public ulong userId { get; set; }
    public string? userName { get; set; }
    public string? question { get; set; }
    public List<VoteByUser>? userVotes { get; set; }
    public bool isOpen { get; set; } = true;
}

public class VoteByUser
{
    public ulong userId { get; set; }
    public string? userName { get; set; }
    // TODO: set it up for complex votes with more choises
    public bool choise { get; set; }
}