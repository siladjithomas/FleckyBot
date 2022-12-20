using System.ComponentModel.DataAnnotations;

namespace Database.Models;

public class Quote
{
    [Key]
    public ulong Id { get; set; }
    [Required]
    public string QuoteText { get; set; } = "";
    [Required]
    public string QuoteAuthor { get; set; } = "";
}

public class QuoteJson
{
    public List<string>? quote { get; set; }
    public string? author { get; set; }
}