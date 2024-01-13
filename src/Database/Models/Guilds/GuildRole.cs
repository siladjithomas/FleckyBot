using System.ComponentModel.DataAnnotations;

namespace Database.Models.Guilds
{
    public class GuildRole
    {
        [Key]
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public string? RoleName { get; set; }
        // Role Importance Reminder:
        // 0 -> member
        // 1 -> ?
        // 2 -> ?
        public int RoleImportance { get; set; }


        public int GuildId { get; set; }
        public Guild? Guild { get; set; }
    }
}
