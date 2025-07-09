using Domain.Models;
using Domain.Models.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor)
    : DbContext(options)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public DbSet<User> Users => Set<User>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is AuditableBaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

        foreach (var entry in entries)
        {
            var entity = (AuditableBaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.Created = DateTime.Now;
                entity.CreatedBy = username;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property("Created").IsModified = false;
                entry.Property("CreatedBy").IsModified = false;

                entity.LastModified = DateTime.Now;
                entity.LastModifiedBy = username;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges() =>
          SaveChangesAsync().GetAwaiter().GetResult();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

