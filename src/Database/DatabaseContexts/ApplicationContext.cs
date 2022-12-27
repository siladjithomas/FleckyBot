using Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Database.DatabaseContexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext() {}

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

    public DbSet<RequestableRole> RequestableRoles { get; set; }

    public DbSet<Guild> Guilds { get; set; }
    public DbSet<GuildSystemMessagesChannel> GuildSystemMessagesChannels { get; set; }
    public DbSet<GuildRolesChannel> GuildRolesChannels { get; set; }
    public DbSet<GuildVotesChannel> GuildVotesChannels { get; set; }
    public DbSet<GuildTicketsChannel> GuildTicketsChannels { get; set; }
    public DbSet<GuildTicketsGroup> GuildTicketsGroups { get; set; }
    public DbSet<Quote> Quote { get; set; }
    public DbSet<Vote> Vote { get; set; }
    public DbSet<VoteUser> VoteUser { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Do stuff here

        modelBuilder.Entity<Guild>(entity => {
            entity.HasOne(t => t.GuildRolesChannel).WithOne().HasForeignKey<GuildRolesChannel>(t => t.Id).IsRequired(false);
            entity.HasOne(t => t.GuildSystemMessagesChannel).WithOne().HasForeignKey<GuildSystemMessagesChannel>(t => t.Id).IsRequired(false);
            entity.HasOne(t => t.GuildTicketsChannel).WithOne().HasForeignKey<GuildTicketsChannel>(t => t.Id).IsRequired(false);
            entity.HasOne(t => t.GuildVotesChannel).WithOne().HasForeignKey<GuildVotesChannel>().IsRequired(false);
        });

        modelBuilder.Entity<GuildTicketsGroup>(entity => {
            entity.HasOne(ut => ut.GuildTicketsChannels).WithMany(t => t.GuildTicketsGroups).HasForeignKey(t => t.Id);
        });

        modelBuilder.Entity<VoteUser>(entity => {
            entity.HasOne(ut => ut.Vote).WithMany(t => t.VoteByUser).HasForeignKey(t => t.Id);
        });

        string text = File.ReadAllText(@"./quotesCollection.json");
        List<QuoteJson>? quotes = JsonSerializer.Deserialize<List<QuoteJson>>(text);
        var quotesInList = new List<Quote>();
        ulong id = 1;

        if (quotes != null)
            foreach (QuoteJson quote in quotes)
            {
                var combinedQuotes = String.Join(@"\n", quote.quote);
                
                quotesInList.Add(new Quote
                {
                    Id = id,
                    QuoteText = combinedQuotes,
                    QuoteAuthor = quote.author
                });

                id++;
            }

        modelBuilder.Entity<Quote>().HasData(quotesInList);
        
        base.OnModelCreating(modelBuilder);
    }
}