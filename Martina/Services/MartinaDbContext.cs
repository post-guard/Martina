using Martina.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Martina.Services;

public sealed class MartinaDbContext(DbContextOptions<MartinaDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }

    public DbSet<Room> Rooms { get; init; }

    public DbSet<CheckinRecord> CheckinRecords { get; init; }

    public DbSet<AirConditionerRecord> AirConditionerRecords { get; init; }

    public DbSet<BillRecord> BillRecords { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .ToCollection("users")
            .HasIndex(user => user.UserId);

        modelBuilder.Entity<Room>()
            .ToCollection("rooms");

        modelBuilder.Entity<CheckinRecord>()
            .ToCollection("checkinRecords");
    }
}
