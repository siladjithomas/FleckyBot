using System.ComponentModel.DataAnnotations;

namespace Database.Models;

public class Vote 
{
    [Key]
    public int Id { get; set; }
    public ulong MessageId { get; set; }
    public string? QuestionText { get; set; }
    public bool isOpen { get; set; } = true;

    public List<VoteUser>? VoteByUser { get; set; }
}

public class VoteUser
{
    [Key]
    public int Id { get; set; }
    public ulong UserId { get; set; }
    public string? UserName { get; set; }
    public bool? UserVote { get; set; }

    public int VoteId { get; set; }
    public Vote? Vote { get; set; }
}