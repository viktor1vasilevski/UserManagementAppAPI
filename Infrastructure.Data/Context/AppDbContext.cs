using Domain.Models;
using Domain.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {

    }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is AuditableBaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                ((AuditableBaseEntity)entityEntry.Entity).Created = DateTime.Now;
                
            }
            else
            {
                Entry((AuditableBaseEntity)entityEntry.Entity).Property(p => p.Created).IsModified = false;
                Entry((AuditableBaseEntity)entityEntry.Entity).Property(p => p.CreatedBy).IsModified = false;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                ((AuditableBaseEntity)entityEntry.Entity).LastModified = DateTime.Now;
                
            }
        }

        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
    public override int SaveChanges() =>
          SaveChangesAsync().GetAwaiter().GetResult();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
