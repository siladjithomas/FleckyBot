using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bot.Models;

public class GuildMemberLevel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public long userId { get; set; }
    public string? userName { get; set; }
    public LevelInGuild[]? guildLevels { get; set; }
}

public class LevelInGuild
{
    public long guildId { get; set; }
    public int points { get; set; }
    public int level { get; set; }
}