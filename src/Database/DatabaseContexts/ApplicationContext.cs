using Database.Models;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Do stuff here

        modelBuilder.Entity<GuildSystemMessagesChannel>(entity => 
        {
            entity.HasOne(ut => ut.Guild).WithOne(t => t.GuildSystemMessagesChannel).HasForeignKey<Guild>(t => t.GuildId);
        });

        modelBuilder.Entity<GuildRolesChannel>(entity => 
        {
            entity.HasOne(ut => ut.Guild).WithOne(t => t.GuildRolesChannel).HasForeignKey<Guild>(t => t.GuildId);
        });

        modelBuilder.Entity<GuildVotesChannel>(entity => 
        {
            entity.HasOne(ut => ut.Guild).WithOne(t => t.GuildVotesChannel).HasForeignKey<Guild>(t => t.GuildId);
        });

        modelBuilder.Entity<GuildTicketsChannel>(entity => 
        {
            entity.HasOne(ut => ut.Guild).WithOne(t => t.GuildTicketsChannel).HasForeignKey<Guild>(t => t.GuildId);
        });

        modelBuilder.Entity<GuildTicketsGroup>(entity => {
            entity.HasKey(ut => new { ut.ChannelId });

            entity.HasOne(ut => ut.GuildTicketsChannel).WithMany(t => t.GuildTicketsGroups).HasForeignKey(t => t.ChannelId);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}