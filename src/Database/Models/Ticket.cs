using System.ComponentModel.DataAnnotations;

namespace Database.Models;

public class Ticket
{
    [Key]
    public int Id { get; set; }
    public ulong UserId { get; set; }
    public string? UserName { get; set; }
    public ulong ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public bool IsOpen { get; set; } = true;
    public DateTime TimestampCreated { get; set; }

    public IEnumerable<TicketMessage>? TicketMessages { get; set; }
}

public class TicketMessage
{
    [Key]
    public int Id { get; set; }
    public ulong UserId { get; set; }
    public string? UserName { get; set; }
    public string? Message { get; set; }
    public DateTime TimestampCreated { get; set; }

    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}