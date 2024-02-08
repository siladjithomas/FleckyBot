using System.ComponentModel.DataAnnotations;

namespace TsubaHaru.FleckyBot.Database.Models;

public class BirthdayUser
{
    [Key]
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public string? GuildName { get; set; }
    public ulong UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Birthday { get; set; }
    public bool IsPublic { get; set; } = false;
}