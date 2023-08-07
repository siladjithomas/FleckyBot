using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

public class BirthdayUser
{
    [Key]
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public DateTime Birthday { get; set; }
    public bool IsPublic { get; set; } = false;
}