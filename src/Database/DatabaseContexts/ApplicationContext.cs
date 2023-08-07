using Database.Models;
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
    public DbSet<GuildVotesChannel>? GuildVotesChannels { get; set; }
    public DbSet<GuildTicketsChannel>? GuildTicketsChannels { get; set; }
    public DbSet<GuildTicketsGroup>? GuildTicketsGroups { get; set; }
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Do stuff here

        modelBuilder.Entity<Guild>(entity => {
            entity.HasOne<GuildRolesChannel>(t => t.GuildRolesChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildRolesChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne<GuildSystemMessagesChannel>(t => t.GuildSystemMessagesChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildSystemMessagesChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne<GuildTicketsChannel>(t => t.GuildTicketsChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildTicketsChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne<GuildVotesChannel>(t => t.GuildVotesChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildVotesChannel>(t => t.Id)
                .IsRequired(false);

            entity.HasOne<GuildSignupChannel>(t => t.GuildSignupChannel)
                .WithOne(r => r.Guild)
                .HasForeignKey<GuildSignupChannel>(t => t.Id)
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