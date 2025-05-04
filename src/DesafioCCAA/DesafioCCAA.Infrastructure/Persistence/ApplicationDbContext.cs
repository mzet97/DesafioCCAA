using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Domains.Identities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DesafioCCAA.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    Guid,
    ApplicationUserClaim,
    ApplicationUserRole,
    ApplicationUserLogin,
    ApplicationRoleClaim,
    ApplicationUserToken>(options)
{

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var entries = ChangeTracker.Entries().Where(e => e.Entity.GetType().GetProperty("CreatedAt") != null);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                entry.Property("CreatedAt").IsModified = true;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("CreatedAt").IsModified = false;
            }

            var updatedAtProp = entry.Entity.GetType().GetProperty("UpdatedAt");
            if (updatedAtProp != null)
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }


        return base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
    public DbSet<ApplicationRole> ApplicationRoles { get; set; } = null!;
    public DbSet<ApplicationUserClaim> ApplicationUserClains { get; set; } = null!;
    public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; } = null!;
    public DbSet<ApplicationRoleClaim> ApplicationRoleClains { get; set; } = null!;
    public DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; } = null!;

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Gender> Genders { get; set; } = null!;
    public DbSet<Publisher> Publishers { get; set; } = null!;
}
