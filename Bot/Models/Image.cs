using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bot.Models;

public class Image
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? url { get; set; }
    public string? type { get; set; }
}