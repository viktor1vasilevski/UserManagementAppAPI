using Domain.Models;
using Domain.Models.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data.Context;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext()
    {

    }
    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<User> Users { get; set; }

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
            else
            {
                Entry(entity).Property(p => p.Created).IsModified = false;
                Entry(entity).Property(p => p.CreatedBy).IsModified = false;
            }

            if (entry.State == EntityState.Modified)
            {
                ((AuditableBaseEntity)entry.Entity).LastModified = DateTime.Now;
                ((AuditableBaseEntity)entry.Entity).LastModifiedBy = username;
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
