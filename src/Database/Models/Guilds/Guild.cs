using Database.Models.SleepCalls;
using System.ComponentModel.DataAnnotations;

namespace Database.Models.Guilds;

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
    public GuildSignupChannel? GuildSignupChannel { get; set; }
    public GuildRuleChannel? GuildRuleChannel { get; set; }

    public List<GuildRole>? ImportantGuildRoles { get; set; }
    public List<GuildRule>? GuildRules { get; set; }

    public List<SleepCallCategory>? SleepCallCategories { get; set; }
    public List<SleepCallIgnoredChannel>? SleepCallIgnoredChannels { get; set; }
}