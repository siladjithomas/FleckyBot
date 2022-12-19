using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database.DatabaseContexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext() {}

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

    public DbSet<RequestableRole> RequestableRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Do stuff here
        
        base.OnModelCreating(modelBuilder);
    }
}