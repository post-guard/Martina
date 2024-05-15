using Microsoft.EntityFrameworkCore;
using Testcontainers.MongoDb;

namespace Martina.Tests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder()
        .WithImage("mongo:7.0-jammy")
        .Build();

    private string ConnectionString => _mongoDbContainer.GetConnectionString();

    public Task InitializeAsync() => _mongoDbContainer.StartAsync();

    public Task DisposeAsync() => _mongoDbContainer.DisposeAsync().AsTask();

    public MartinaDbContext CreateDbContext()
    {
        DbContextOptionsBuilder<MartinaDbContext> builder = new();
        builder.UseMongoDB(ConnectionString, "martina-test");

        return new MartinaDbContext(builder.Options);
    }
}
