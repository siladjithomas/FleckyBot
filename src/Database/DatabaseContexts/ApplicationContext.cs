using Database.Models;
using Database.Models.Guilds;
using Database.Models.SleepCalls;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Database.DatabaseContexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext() {}

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

    public DbSet<RequestableRole>? RequestableRoles { get; set; }

    public DbSet<Guild>? Guilds { get; set; }
    public DbSet<GuildSystemMessagesChannel>? GuildSystemMessagesChannels { get; set; }
    public DbSet<GuildRolesChannel>? GuildRolesChannels { get; set; }
    public DbSet<GuildRuleChannel>? GuildRuleChannels { get; set; }
    public DbSet<GuildVotesChannel>? GuildVotesChannels { get; set; }
    public DbSet<GuildTicketsChannel>? GuildTicketsChannels { get; set; }
    public DbSet<GuildTicketsGroup>? GuildTicketsGroups { get; set; }
    public DbSet<GuildTimetableLine>? GuildTimetableLines { get; set; }
    public DbSet<GuildRole>? ImportantGuildRoles { get; set; }
    public DbSet<GuildRule>? GuildRules { get; set; }
    public DbSet<Quote>? Quote { get; set; }
    public DbSet<Vote>? Vote { get; set; }
    public DbSet<VoteUser>? VoteUser { get; set; }
    public DbSet<Ticket>? Ticket { get; set; }
    public DbSet<TicketMessage>? TicketMessage { get; set; } 
    public DbSet<Image>? Images { get; set; }
	public DbSet<Signup>? Signup { get; set; }
	public DbSet<SignupAllowedGroup>? SignupAllowedGroup { get; set; }
	public DbSet<SignupAllowedToEdit>? SignupAllowedToEdit { get; set; }
	public DbSet<SignupCategory>? SignupCategory { get; set; }
	public DbSet<SignupAttendee>? SignupAttendee { get; set; }

    // Telegram related tables
    public DbSet<TelegramUser>? TelegramUser { get; set; }
    public DbSet<TelegramChat>? TelegramChat { get; set; }
    public DbSet<TelegramMessage>? TelegramMessage { get; set; }

    // Birthday related tables
    public DbSet<BirthdayUser>? BirthdayUser { get; set; }

    // Sleep Channels related tables
    public DbSet<SleepCallCategory>? SleepCallCategorys { get; set; }
    public DbSet<SleepCallGroup> SleepCallGroups { get; set; }
    public DbSet<SleepCallActiveChannel> SleepCallActiveChannels { get; set; }
    public DbSet<SleepCallIgnoredChannel> SleepCallIgnoredChannels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Do stuff here

        modelBuilder.Entity<Guild>(entity => {
            entity.HasOne(t => t.GuildRolesChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildRolesChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne(t => t.GuildSystemMessagesChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildSystemMessagesChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne(t => t.GuildTicketsChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildTicketsChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne(t => t.GuildVotesChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildVotesChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne(t => t.GuildSignupChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildSignupChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne(t => t.GuildRuleChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildRuleChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne(t => t.GuildTimetableChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildTimetableChannel>(t => t.Id)
                .IsRequired(false);
        });

        modelBuilder.Entity<GuildRole>(entity =>
        {
            entity.HasOne(t => t.Guild)
                .WithMany(t => t.ImportantGuildRoles)
                .HasForeignKey(t => t.GuildId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<GuildRule>(entity => 
        { 
            entity.HasOne(t => t.Guild)
                .WithMany(t => t.GuildRules)
                .HasForeignKey(t => t.GuildId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<GuildTimetableLine>(entity => 
        { 
            entity.HasOne(t => t.Guild)
                .WithMany(t => t.GuildTimetableLines)
                .HasForeignKey(t => t.GuildId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<GuildTicketsGroup>(entity => {
            entity.HasOne(ut => ut.GuildTicketsChannel)
                .WithMany(t => t.GuildTicketsGroups)
                .HasForeignKey(t => t.GuildTicketsChannelId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<VoteUser>(entity => {
            entity.HasOne(ut => ut.Vote)
                .WithMany(t => t.VoteByUser)
                .HasForeignKey(t => t.VoteId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<TicketMessage>(entity => {
            entity.HasOne(t => t.Ticket)
                .WithMany(t => t.TicketMessages)
                .HasForeignKey(t => t.TicketId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

		modelBuilder.Entity<SignupCategory>(entity => {
			entity.HasOne(s => s.Signup)
				.WithMany(s => s.SignupCategories)
				.HasForeignKey(s => s.SignupId)
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired(false);
		});

		modelBuilder.Entity<SignupAllowedToEdit>(entity => {
			entity.HasOne(s => s.Signup)
				.WithMany(s => s.SignupAllowedToEdit)
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired(false);
		});

		modelBuilder.Entity<SignupAllowedGroup>(entity => {
			entity.HasOne(s => s.Signup)
				.WithMany(s => s.SignupRestrictedTo)
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired(false);
		});

        modelBuilder.Entity<TelegramMessage>(entity => {
            entity.HasOne(s => s.Chat)
                .WithMany(s => s.Messages)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<TelegramChat>(entity => {
            entity.HasOne(s => s.User)
                .WithMany(s => s.Chats)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<SleepCallCategory>(entity =>
        {
            entity.HasOne(t => t.Guild)
                .WithMany(t => t.SleepCallCategories)
                .HasForeignKey(t => t.GuildId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<SleepCallIgnoredChannel>(entity =>
        {
            entity.HasOne(t => t.Guild)
                .WithMany(t => t.SleepCallIgnoredChannels)
                .HasForeignKey(t => t.GuildId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<SleepCallGroup>(entity =>
        {
            entity.HasOne(t => t.SleepCallCategory)
                .WithMany(t => t.SleepCallGroups)
                .HasForeignKey(t => t.SleepCallCategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<SleepCallActiveChannel>(entity =>
        {
            entity.HasOne(t => t.SleepCallCategory)
                .WithMany(t => t.SleepCallActiveChannels)
                .HasForeignKey(t => t.SleepCallCategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        string textQuotes = File.ReadAllText(@"./quotesCollection.json");
        List<QuoteJson>? quotes = JsonSerializer.Deserialize<List<QuoteJson>>(textQuotes);
        var quotesInList = new List<Quote>();
        ulong id = 1;

        if (quotes != null)
            foreach (QuoteJson quote in quotes)
            {
                var combinedQuotes = "";
                
                if (quote.quote != null)
                    combinedQuotes = String.Join(@"\n", quote.quote);
                
                if (quote.author != null)
                    quotesInList.Add(new Quote
                    {
                        Id = id,
                        QuoteText = combinedQuotes,
                        QuoteAuthor = quote.author
                    });

                id++;
            }

        modelBuilder.Entity<Quote>().HasData(quotesInList);

        string textImages = File.ReadAllText(@"./imagesCollection.json");
        List<Image>? images = JsonSerializer.Deserialize<List<Image>>(textImages);

        if (images != null)
            modelBuilder.Entity<Image>().HasData(images);
        
        base.OnModelCreating(modelBuilder);
    }
}