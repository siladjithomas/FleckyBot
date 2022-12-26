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

        modelBuilder.Entity<GuildSystemMessagesChannel>(entity => 
        {
            entity.HasOne(ut => ut.Guild).WithOne(t => t.GuildSystemMessagesChannel).HasForeignKey<Guild>(t => t.Id);
        });

        modelBuilder.Entity<GuildRolesChannel>(entity => 
        {
            entity.HasOne(ut => ut.Guild).WithOne(t => t.GuildRolesChannel).HasForeignKey<Guild>(t => t.Id);
        });

        modelBuilder.Entity<GuildVotesChannel>(entity => 
        {
            entity.HasOne(ut => ut.Guild).WithOne(t => t.GuildVotesChannel).HasForeignKey<Guild>(t => t.Id);
        });

        modelBuilder.Entity<GuildTicketsChannel>(entity => 
        {
            entity.HasOne(ut => ut.Guild).WithOne(t => t.GuildTicketsChannel).HasForeignKey<Guild>(t => t.Id);
        });

        modelBuilder.Entity<GuildTicketsGroup>(entity => {
            entity.HasOne(ut => ut.GuildTicketsChannel).WithMany(t => t.GuildTicketsGroups).HasForeignKey(t => t.Id);
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