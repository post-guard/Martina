using Martina.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Martina.Services;

public sealed class MartinaDbContext(DbContextOptions<MartinaDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }

    public DbSet<UserPermission> UserPermissions { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .ToCollection("users")
            .HasIndex(user => user.UserId);

        modelBuilder.Entity<UserPermission>()
            .ToCollection("userPermissions")
            .HasIndex(permission => permission.UserId);
    }
}
