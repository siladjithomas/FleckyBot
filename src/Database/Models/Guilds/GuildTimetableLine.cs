using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models.Guilds
{
    public class GuildTimetableLine
    {
        [Key]
        public int Id { get; set; }
        public ulong RequestingUserId { get; set; }
        public string? RequestingUserName { get; set; }
        public DateTime? RequestedTime { get; set; }
        public bool IsApproved { get; set; } = false;

        public int GuildId { get; set; }
        public Guild? Guild { get; set; }
    }
}
