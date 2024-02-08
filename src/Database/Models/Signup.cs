using System.ComponentModel.DataAnnotations;

namespace TsubaHaru.FleckyBot.Database.Models;

public class Signup
{
	[Key]
	public int Id { get; set; }
	public long RoleId { get; set; }
	[MaxLength(90)]
	public string Name { get; set; } = String.Empty;
	public List<SignupAllowedGroup> SignupRestrictedTo { get; set; } = new List<SignupAllowedGroup>();
	public List<SignupAllowedToEdit> SignupAllowedToEdit { get; set; } = new List<SignupAllowedToEdit>();
	public List<SignupCategory> SignupCategories { get; set; } = new List<SignupCategory>();
	public DateTime EventStart { get; set; }
	public DateTime EventEnd { get; set; }
	public bool isOnDiscord { get; set; }
	public long CreatedBy { get; set; }
	public DateTime CreatedOn { get; set; }
	public long ChangedBy { get; set; }
	public DateTime ChangedOn { get; set; }
}

public class SignupCategory
{
	[Key]
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public List<SignupAttendee> Attendees { get; set; } = new List<SignupAttendee>();

	public int SignupId { get; set; }
	public Signup Signup { get; set; } = new Signup();
}

public class SignupAllowedToEdit
{
	[Key]
	public int Id { get; set; }
	public long GroupId { get; set; }

	public int SignupId { get; set; }
	public Signup? Signup { get; set; }
}

public class SignupAllowedGroup
{
	[Key]
	public int Id { get; set; }
	public long GroupId { get; set; }

	public int SignupId { get; set; }
	public Signup? Signup { get; set; }
}

public class SignupAttendee
{
	[Key]
	public int Id { get; set; }
	public long GroupId { get; set; }

	public int SignupId { get; set; }
	public Signup? Signup { get; set; }
}