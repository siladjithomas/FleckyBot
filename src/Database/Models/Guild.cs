using System.ComponentModel.DataAnnotations;

namespace Database.Models;

public class Guild
{
    [Key]
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public string? GuildName { get; set; }
    public ulong GuildAdminId { get; set; }
    public string? GuildAdminName { get; set; }

    public GuildSystemMessagesChannel? GuildSystemMessagesChannel { get; set; }
    public GuildRolesChannel? GuildRolesChannel { get; set; }
    public GuildVotesChannel? GuildVotesChannel { get; set; }
    public GuildTicketsChannel? GuildTicketsChannel { get; set; }
}

public class GuildSystemMessagesChannel
{
    [Key]
    public int Id { get; set; }
    public ulong ChannelId { get; set; }
    public string? ChannelName { get; set; }

    public int GuildId { get; set; }
    public Guild? Guild { get; set; }
}

public class GuildRolesChannel
{
    [Key]
    public int Id { get; set; }
    public ulong ChannelId { get; set; }
    public string? ChannelName { get; set; }

    public int GuildId { get; set; }
    public Guild? Guild { get; set; }
}

public class GuildVotesChannel
{
    [Key]
    public int Id { get; set; }
    public ulong ChannelId { get; set; }
    public string? ChannelName { get; set; }

    public int GuildId { get; set; }
    public Guild? Guild { get; set; }
}

public class GuildTicketsChannel 
{
    [Key]
    public int Id { get; set; }
    public ulong ChannelId { get; set; }
    public string? ChannelName { get; set; }
    
    public List<GuildTicketsGroup>? GuildTicketsGroups { get; set; }
    public int GuildId { get; set; }
    public Guild? Guild { get; set; }
}

public class GuildTicketsGroup
{
    [Key]
    public int Id { get; set; }
    public ulong GroupId { get; set; }
    public string? GroupName { get; set; }
    public string? GroupType { get; set; }

    public GuildTicketsChannel? GuildTicketsChannel { get; set; }
    public int GuildTicketChannelId { get; set; }
}