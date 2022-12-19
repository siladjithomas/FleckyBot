using Microsoft.EntityFrameworkCore;

namespace DatabaseContexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext() {}

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Do stuff here
        
        base.OnModelCreating(modelBuilder);
    }
}