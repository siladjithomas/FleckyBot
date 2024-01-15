using System.ComponentModel.DataAnnotations;

namespace Database.Models.Guilds
{
    public class GuildRule
    {
        [Key]
        public int Id { get; set; }
        public string? RuleText { get; set; }
        [MaxLength(2)]
        public string? RuleLanguage { get; set; }

        public int GuildId { get; set; }
        public Guild? Guild { get; set; }
    }
}
