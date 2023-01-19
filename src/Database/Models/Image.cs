using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

public class Image
{
    [Key]
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Uri { get; set; }
}