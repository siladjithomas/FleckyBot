using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bot.Models;

public class Guild
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public ulong GuildId { get; set; }
    public string? GuildName { get; set; }
    public ulong GuildAdminId { get; set; }
    public string? GuildAdminName { get; set; }
    public GuildSystemMessagesChannel GuildSystemMessagesChannel { get; set; } = new GuildSystemMessagesChannel();
    public GuildVotesChannel GuildVotesChannel { get; set; } = new GuildVotesChannel();
    public GuildTicketChannel GuildTicketChannel { get; set; } = new GuildTicketChannel();
    public GuildRolesChannel GuildRolesChannel { get; set; } = new GuildRolesChannel();
    public GuildRoles[] GuildRoles { get; set; } = new [] { new GuildRoles() };
}

public class GuildRolesChannel
{
    public ulong ChannelId { get; set; }
    public string? ChannelName { get; set; }
}

public class GuildRoles
{
    public string? RoleDisplayName { get; set; }
    public string? RoleDescription { get; set; }
    public bool IsMultipleChoice { get; set; }
    public bool IsEnabled { get; set; } = false;
    public SelectableRoles[]? SelectableRoles { get; set; }
}

public class SelectableRoles
{
    public ulong RoleId { get; set; }
    public string? RoleName { get; set; }
}

public class GuildSystemMessagesChannel
{
    public ulong ChannelId { get; set; }
    public string? ChannelName { get; set; }
}

public class GuildVotesChannel
{
    public ulong ChannelId { get; set; }
    public string? ChannelName { get; set; }
}

public class GuildTicketChannel
{
    public ulong CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<GuildTicketGroup>? GuildTicketGroups { get; set; }
}

public class GuildTicketGroup
{
    public ulong GroupId { get; set; }
    public string? GroupName { get; set; }
    // Currently possible: admin, mod
    public string? GroupType { get; set; }
}