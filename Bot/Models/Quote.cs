using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bot.Models;

public class Quote
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public List<string> quote { get; set; } = new List<string>();
    public string? author { get; set; }
}

