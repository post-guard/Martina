using System.Collections.Concurrent;
using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

public class DefaultSchedular(IServiceProvider provider) : BackgroundService, ISchedular
{
    public ConcurrentDictionary<ObjectId, AirConditionerState> States { get; } = [];

    public Task SendRequest(ObjectId roomId, AirConditionerRequest request)
    {
        return Task.CompletedTask;
    }

    public Task Reset()
    {
        using IServiceScope scope = provider.CreateScope();
        using MartinaDbContext dbContext = scope.ServiceProvider.GetRequiredService<MartinaDbContext>();

        States.Clear();

        foreach (Room room in dbContext.Rooms.AsNoTracking())
        {
            States.TryAdd(room.Id, new AirConditionerState(room, true));
        }

        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Reset();
    }
}
