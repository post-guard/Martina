using System.Collections.Concurrent;
using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Enums;
using Martina.Extensions;
using Martina.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Martina.Services;

public class BuptSchedular(
    IServiceProvider provider,
    IOptions<TimeOption> timeOpton,
    AirConditionerManageService airConditionerManageService,
    ILogger<BuptSchedular> logger
) : BackgroundService, ISchedular
{
    private readonly LinkedList<AirConditionerService> _serviceQueue = [];

    private readonly LinkedList<AirConditionerService> _waitingQueue = [];

    private readonly ConcurrentQueue<AirConditionerService> _pendingRequests = [];

    public ConcurrentDictionary<ObjectId, AirConditionerState> States { get; } = [];

    public Task SendRequest(ObjectId roomId, AirConditionerRequest request)
    {
        AirConditionerService service = new(airConditionerManageService.Rooms[roomId], request);
        _pendingRequests.Enqueue(service);

        return Task.CompletedTask;
    }

    public async Task Reset()
    {
        using IServiceScope scope = provider.CreateScope();
        await using MartinaDbContext context =
            scope.ServiceProvider.GetRequiredService<MartinaDbContext>();

        logger.LogInformation("Reset air conditioner controller system.");

        States.Clear();
        airConditionerManageService.Rooms.Clear();
        _serviceQueue.Clear();
        _waitingQueue.Clear();

        foreach (Room room in context.Rooms)
        {
            airConditionerManageService.Rooms.TryAdd(room.Id, room);
            States.TryAdd(room.Id, new AirConditionerState(room, airConditionerManageService.Option.Cooling));
        }
    }

    private async Task Schedule(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                UpdateTemperature();

                HandleOnTargetTemperature();

                HandlePendingRequests();

                RemoveUptoTimeService();

                HandleShutdownRequest();

                PrioritySchedule();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// 处理达到目标温度的请求
    /// </summary>
    private void HandleOnTargetTemperature()
    {
        LinkedListNode<AirConditionerService>? node = _serviceQueue.First;

        while (node is not null)
        {
            AirConditionerState state = States[node.Value.Room.Id];

            if (state.Cooling
                    ? state.CurrentTemperature <= state.TargetTemperature
                    : state.CurrentTemperature >= state.TargetTemperature)
            {
                // 到达目标温度
                state.Status = AirConditionerStatus.Closed;

                LinkedListNode<AirConditionerService> removed = node;
                node = node.Next;
                _serviceQueue.Remove(removed);
            }
            else
            {
                node = node.Next;
            }
        }

        while (_serviceQueue.Count < 3 && _waitingQueue.First is not null)
        {
            AirConditionerService service = _waitingQueue.First.Value;
            AirConditionerState state = States[service.Room.Id];

            state.Status = AirConditionerStatus.Working;
            _serviceQueue.AddLast(service);
            _waitingQueue.RemoveFirst();
        }
    }

    /// <summary>
    /// 处理缓冲队列中的请求
    /// </summary>
    private void HandlePendingRequests()
    {
        while (_pendingRequests.TryDequeue(out AirConditionerService? pendingService))
        {
            // 是否已经存在与服务队列或者等待队列中
            bool existed = false;
            AirConditionerState state = States[pendingService.Room.Id];
            state.TargetTemperature = pendingService.TargetTemperature;
            state.Speed = pendingService.Speed;

            LinkedListNode<AirConditionerService>? node = _serviceQueue.First;

            while (node is not null)
            {
                if (node.Value.Room.Id == pendingService.Room.Id)
                {
                    node.Value = pendingService;
                    existed = true;
                    break;
                }

                node = node.Next;
            }

            node = _waitingQueue.First;

            while (node is not null)
            {
                if (existed)
                {
                    break;
                }

                if (node.Value.Room.Id == pendingService.Room.Id)
                {
                    node.Value = pendingService;
                    existed = true;
                    break;
                }

                node = node.Next;
            }

            if (!existed)
            {
                _waitingQueue.AddLast(pendingService);
                state.Status = AirConditionerStatus.Waiting;
            }
        }
    }

    /// <summary>
    /// 移除达到时间片的请求
    /// </summary>
    private void RemoveUptoTimeService()
    {
        LinkedListNode<AirConditionerService>? node = _serviceQueue.First;

        // 从服务队列中将时间片到达的服务加入等待队列
        while (node is not null)
        {
            AirConditionerState state = States[node.Value.Room.Id];

            if (node.Value.TimeToLive == 0)
            {
                LinkedListNode<AirConditionerService> removedNode = node;
                node = node.Next;
                removedNode.Value.TimeToLive = 20;
                state.Status = AirConditionerStatus.Waiting;
                _waitingQueue.AddLast(removedNode.Value);
                _serviceQueue.Remove(removedNode);
            }
            else
            {
                node = node.Next;
            }
        }

        while (_serviceQueue.Count < 3 && _waitingQueue.First is not null)
        {
            AirConditionerService service = _waitingQueue.First.Value;
            AirConditionerState state = States[service.Room.Id];

            state.Status = AirConditionerStatus.Working;
            _serviceQueue.AddLast(service);
            _waitingQueue.RemoveFirst();
        }
    }

    /// <summary>
    /// 处理关机的请求
    /// </summary>
    private void HandleShutdownRequest()
    {
        LinkedListNode<AirConditionerService>? node = _serviceQueue.First;

        while (node is not null)
        {
            if (!node.Value.Opening)
            {
                AirConditionerState state = States[node.Value.Room.Id];
                state.Status = AirConditionerStatus.Closed;
                LinkedListNode<AirConditionerService> removedNode = node;
                node = node.Next;
                _serviceQueue.Remove(removedNode);
            }
            else
            {
                node = node.Next;
            }
        }

        node = _waitingQueue.First;

        while (node is not null)
        {
            if (!node.Value.Opening)
            {
                AirConditionerState state = States[node.Value.Room.Id];
                state.Status = AirConditionerStatus.Closed;
                LinkedListNode<AirConditionerService> removedNode = node;
                node = node.Next;
                _waitingQueue.Remove(removedNode);
            }
            else
            {
                node = node.Next;
            }
        }

        while (_serviceQueue.Count < 3 && _waitingQueue.First is not null)
        {
            AirConditionerService service = _waitingQueue.First.Value;
            AirConditionerState state = States[service.Room.Id];

            state.Status = AirConditionerStatus.Working;
            _serviceQueue.AddLast(service);
            _waitingQueue.RemoveFirst();
        }
    }

    /// <summary>
    /// 进行优先级的调度
    /// </summary>
    private void PrioritySchedule()
    {
        LinkedListNode<AirConditionerService>? waitingNode = _waitingQueue.First;

        while (waitingNode is not null)
        {
            LinkedListNode<AirConditionerService>? node = _serviceQueue.First;

            while (node is not null)
            {
                if (node.Value.Speed < waitingNode.Value.Speed)
                {
                    // 找到一个优先级比他低

                    // 将服务队列中的移动到等待队列的末尾
                    _waitingQueue.AddLast(node.Value);
                    AirConditionerState state = States[node.Value.Room.Id];
                    state.Status = AirConditionerStatus.Waiting;
                    _serviceQueue.Remove(node);

                    // 将等待队列中的移动到服务队列的末尾
                    _serviceQueue.AddLast(waitingNode.Value);
                    state = States[node.Value.Room.Id];
                    state.Status = AirConditionerStatus.Working;
                    LinkedListNode<AirConditionerService> removedNode = waitingNode;
                    waitingNode = waitingNode.Next;
                    _waitingQueue.Remove(removedNode);

                    break;
                }

                node = node.Next;
            }
        }
    }

    /// <summary>
    /// 更新各个房间的温度
    /// </summary>
    private void UpdateTemperature()
    {
        logger.LogServiceQueue("Current service queue: ", _serviceQueue);
        logger.LogServiceQueue("Current waiting queue: ", _waitingQueue);

        LinkedListNode<AirConditionerService>? node = _serviceQueue.First;

        while (node is not null)
        {
            AirConditionerState state = States[node.Value.Room.Id];

            decimal positive = state.Cooling ? -1 : 1;
            switch (node.Value.Speed)
            {
                case FanSpeed.High:
                    state.CurrentTemperature +=
                        positive * 1 / airConditionerManageService.Option.HighSpeedPerDegree / 60 *
                        timeOpton.Value.Factor;
                    break;
                case FanSpeed.Middle:
                    state.CurrentTemperature +=
                        positive * 1 / airConditionerManageService.Option.MiddleSpeedPerDegree / 60 *
                        timeOpton.Value.Factor;
                    break;
                case FanSpeed.Low:
                    state.CurrentTemperature +=
                        positive * 1 / airConditionerManageService.Option.LowSpeedPerDegree / 60 *
                        timeOpton.Value.Factor;
                    break;
            }

            node.Value.TimeToLive -= 1;

            if (state.OnTarget)
            {
                // 到达目标温度
                state.Status = AirConditionerStatus.Closed;
                LinkedListNode<AirConditionerService> removed = node;
                node = node.Next;
                _serviceQueue.Remove(removed);
            }
            else
            {
                node = node.Next;
            }
        }

        foreach (AirConditionerState state in States.Values)
        {
            if (state.Status != AirConditionerStatus.Working)
            {
                if (state.Cooling
                        ? state.CurrentTemperature < state.Room.RoomBasicTemperature
                        : state.CurrentTemperature > state.Room.RoomBasicTemperature)
                {
                    decimal positive = state.Cooling ? 1 : -1;

                    state.CurrentTemperature += airConditionerManageService.Option.BackSpeed / 60 *
                                                timeOpton.Value.Factor * positive;
                }
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Reset();

        await Schedule(stoppingToken);
    }
}
