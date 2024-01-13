using System.ComponentModel.DataAnnotations;

namespace Database.Models.SleepCalls
{
    public class SleepCallGroup
    {
        [Key]
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool IsDeleted { get; set; } = false;

        public int SleepCallCategoryId { get; set; }
        public SleepCallCategory? SleepCallCategory { get; set; }
    }
}
