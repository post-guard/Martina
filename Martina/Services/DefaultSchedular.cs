using System.Collections.Concurrent;
using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Enums;
using Martina.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

public class DefaultSchedular(
    IServiceProvider provider,
    ILogger<DefaultSchedular> logger) : BackgroundService, ISchedular
{
    private record AirConditionerSerivce(ObjectId RoomId, float TargetTemperature, FanSpeed Speed);

    private readonly Dictionary<ObjectId, AirConditionerSerivce> _serviceQueue = [];

    public ConcurrentDictionary<ObjectId, AirConditionerState> States { get; } = [];

    public Task SendRequest(ObjectId roomId, AirConditionerRequest request)
    {
        AirConditionerState state = States[roomId];

        if (!request.Open)
        {
            if (_serviceQueue.ContainsKey(roomId))
            {
                logger.LogInformation("User request to close air conditioner in {}", state.RoomName);
                _serviceQueue.Remove(roomId);
            }

            return Task.CompletedTask;
        }

        if (_serviceQueue.ContainsKey(roomId))
        {
            _serviceQueue[roomId] = new AirConditionerSerivce(roomId, request.TargetTemperature, request.Speed);
        }
        else
        {
            _serviceQueue.Add(roomId, new AirConditionerSerivce(roomId, request.TargetTemperature, request.Speed));
        }

        state.Openning = true;
        state.TargetTemperature = request.TargetTemperature;
        state.Speed = request.Speed;

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

        await UpdateTemperature(stoppingToken);
    }

    private async Task UpdateTemperature(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                List<ObjectId> removed = [];

                foreach (AirConditionerSerivce service in _serviceQueue.Values)
                {
                    AirConditionerState state = States[service.RoomId];

                    if (float.Abs(state.CurrentTemperature - state.TargetTemperature) < AirConditionerOption.Tolerance)
                    {
                        removed.Add(state.RoomId);
                        continue;
                    }

                    switch (service.Speed)
                    {
                        case FanSpeed.High:
                            state.CurrentTemperature -= AirConditionerOption.HighSpeed;
                            break;
                        case FanSpeed.Middle:
                            state.CurrentTemperature -= AirConditionerOption.MiddleSpeed;
                            break;
                        case FanSpeed.Low:
                            state.CurrentTemperature -= AirConditionerOption.LowSpeed;
                            break;
                    }
                }

                foreach (AirConditionerState state in States.Values)
                {
                    if (!state.Openning && float.Abs(state.CurrentTemperature - state.BasicTemperature) >=
                        AirConditionerOption.Tolerance)
                    {
                        state.CurrentTemperature += AirConditionerOption.BackSpeed;
                    }
                }

                foreach (ObjectId id in removed)
                {
                    _serviceQueue.Remove(id);

                    States[id].Openning = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
