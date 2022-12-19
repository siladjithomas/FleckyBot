using System.ComponentModel.DataAnnotations;

namespace Database.Models;

public class Guild
{
    [Key]
    public ulong GuildId { get; set; }
    public string GuildName { get; set; }
    public ulong GuildAdminId { get; set; }
    public string GuildAdminName { get; set; }

    public GuildSystemMessagesChannel GuildSystemMessagesChannel { get; set; } = new GuildSystemMessagesChannel();
    public GuildRolesChannel GuildRolesChannel { get; set; } = new GuildRolesChannel();
    public GuildVotesChannel GuildVotesChannel { get; set; } = new GuildVotesChannel();
    public GuildTicketsChannel GuildTicketsChannel { get; set; } = new GuildTicketsChannel();
}

public class GuildSystemMessagesChannel
{
    [Key]
    public ulong ChannelId { get; set; }
    [Required]
    public string ChannelName { get; set; }

    public ulong GuildId { get; set; }
    public Guild Guild { get; set; }
}

public class GuildRolesChannel
{
    [Key]
    public ulong ChannelId { get; set; }
    [Required]
    public string ChannelName { get; set; }

    public ulong GuildId { get; set; }
    public Guild Guild { get; set; }
}

public class GuildVotesChannel
{
    [Key]
    public ulong ChannelId { get; set; }
    [Required]
    public string ChannelName { get; set; }

    public ulong GuildId { get; set; }
    public Guild Guild { get; set; }
}

public class GuildTicketsChannel 
{
    [Key]
    public ulong ChannelId { get; set; }
    [Required]
    public string ChannelName { get; set; }
    
    public List<GuildTicketsGroup> GuildTicketsGroups { get; set; }
    public ulong GuildId { get; set; }
    public Guild Guild { get; set; }
}

public class GuildTicketsGroup
{
    [Key]
    public ulong GroupId { get; set; }
    [Required]
    public string GroupName { get; set; }
    [Required]
    public string GroupType { get; set; }

    public GuildTicketsChannel GuildTicketsChannel { get; set; }
    public ulong ChannelId { get; set; }
}