using System.ComponentModel.DataAnnotations;

namespace TsubaHaru.FleckyBot.Database.Models.Guilds
{
    public class GuildRole
    {
        [Key]
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public string? RoleName { get; set; }
        // "verified" -> for use when a user gets verified
        // "unverified" -> for use when a user joins the guild and is not verified
        // "mod" -> a mod role
        // "admin" -> a admin role
        public string? RoleDescription { get; set; }


        public int GuildId { get; set; }
        public Guild? Guild { get; set; }
    }
}
