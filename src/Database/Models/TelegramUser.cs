using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

public class TelegramUser
{
    [Key]
    public int Id { get; set; }
    public long? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }

    public List<TelegramChat>? Chats;
    
}

public class TelegramChat
{
    [Key]
    public int Id { get; set; }
    public long? ChatId { get; set; }
    public string? ChatUsername { get; set; }
    public string? ChatBio { get; set; }
    public string? ChatDescription { get; set; }
    public string? ChatType { get; set; }

    public List<TelegramMessage>? Messages { get; set; }
    
    public int UserId { get; set; }
    public TelegramUser? User { get; set; }
}

public class TelegramMessage
{
    [Key]
    public int Id { get; set; }
    public string? Message { get; set; }

    public int ChatId { get; set; }
    public TelegramChat? Chat { get; set; }
}