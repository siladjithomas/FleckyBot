using System.ComponentModel.DataAnnotations;

namespace TsubaHaru.FleckyBot.Database.Models;

public class Image
{
    [Key]
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Uri { get; set; }
}