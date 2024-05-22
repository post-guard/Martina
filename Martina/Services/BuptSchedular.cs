﻿using System.Collections.Concurrent;
using System.Threading.Channels;
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

    private readonly Channel<AirConditionerRecord> _recordsChannel =
        Channel.CreateUnbounded<AirConditionerRecord>(new UnboundedChannelOptions
        {
            SingleReader = true
        });

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
            States.TryAdd(room.Id, new AirConditionerState(room, airConditionerManageService.Option));
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Reset();

        // 下面这些任务都是并行运行的
        List<Task> runingTasks =
        [
            Schedule(stoppingToken),
            WirteRecordToDatabase(stoppingToken)
        ];

        await Task.WhenAll(runingTasks);
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

    private async Task WirteRecordToDatabase(CancellationToken stoppingToken)
    {
        using IServiceScope scope = provider.CreateScope();
        await using MartinaDbContext context = scope.ServiceProvider.GetRequiredService<MartinaDbContext>();

        while (await _recordsChannel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (_recordsChannel.Reader.TryRead(out AirConditionerRecord? record))
            {
                if (record.BeginTemperature == record.EndTemperature)
                {
                    // 跳过没有温差的详单
                    continue;
                }

                // 首先执行一些较为耗时的计算任务
                record.Id = ObjectId.GenerateNewId();
                record.Price = airConditionerManageService.Option.PricePerDegree;
                record.Fee = decimal.Abs(record.BeginTemperature - record.EndTemperature) * record.Price;

                await context.AirConditionerRecords.AddAsync(record, stoppingToken);
                await context.SaveChangesAsync(stoppingToken);
            }
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
                // TODO: 在到达目标温度之后该干嘛？
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

        FillServingQueue();
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
            if (node.Value.TimeToLive == 0)
            {
                LinkedListNode<AirConditionerService> removedNode = node;
                node = node.Next;

                removedNode.Value.TimeToLive = 20;
                MoveToWaitingQueue(removedNode);
            }
            else
            {
                node = node.Next;
            }
        }

        FillServingQueue();
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
                LinkedListNode<AirConditionerService> removedNode = node;
                node = node.Next;

                AirConditionerState state = States[removedNode.Value.Room.Id];
                AirConditionerService service = removedNode.Value;
                state.Status = AirConditionerStatus.Closed;
                _serviceQueue.Remove(removedNode);
                // 记录空调详单
                _recordsChannel.Writer.TryWrite(new AirConditionerRecord
                {
                    RoomId = service.Room.Id,
                    BeginTemperature = service.BeginTemperature,
                    EndTemperature = state.CurrentTemperature,
                    BeginTime = service.BeginTime,
                    EndTime = TimeService.Now,
                    Speed = service.Speed
                });
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
                LinkedListNode<AirConditionerService> removedNode = node;
                node = node.Next;

                AirConditionerState state = States[removedNode.Value.Room.Id];
                state.Status = AirConditionerStatus.Closed;

                _waitingQueue.Remove(removedNode);
            }
            else
            {
                node = node.Next;
            }
        }
    }

    /// <summary>
    /// 进行优先级的调度
    /// </summary>
    private void PrioritySchedule()
    {
        bool changed = true;

        while (changed)
        {
            LinkedListNode<AirConditionerService>? waitingNode = _waitingQueue.First;
            changed = false;

            if (waitingNode is null)
            {
                break;
            }

            LinkedListNode<AirConditionerService>? node = _serviceQueue.First;

            while (node is not null)
            {
                if (node.Value.Speed < waitingNode.Value.Speed)
                {
                    // 找到一个优先级小于等待队列的
                    changed = true;

                    // 将服务队列中的移动到等待队列末尾
                    MoveToWaitingQueue(node);

                    // 将等待队列中的移动到服务队列的末尾
                    MoveToWorkingQueue(waitingNode);
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

            node = node.Next;
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

    /// <summary>
    /// 使用等待队列填充满服务队列
    /// </summary>
    private void FillServingQueue()
    {
        while (_serviceQueue.Count < 3 && _waitingQueue.First is not null)
        {
            MoveToWorkingQueue(_waitingQueue.First);
        }
    }

    /// <summary>
    /// 将服务队列中的一个节点移动到等待队列末尾
    /// </summary>
    /// <param name="workingNode">服务队列中的一个节点</param>
    private void MoveToWaitingQueue(LinkedListNode<AirConditionerService> workingNode)
    {
        AirConditionerState state = States[workingNode.Value.Room.Id];
        state.Status = AirConditionerStatus.Waiting;

        AirConditionerService service = workingNode.Value;
        AirConditionerRecord record = new()
        {
            RoomId = state.Room.Id,
            BeginTime = service.BeginTime,
            EndTime = TimeService.Now,
            Speed = service.Speed,
            BeginTemperature = service.BeginTemperature,
            EndTemperature = state.CurrentTemperature
        };
        _recordsChannel.Writer.TryWrite(record);

        _waitingQueue.AddLast(workingNode.Value);
        _serviceQueue.Remove(workingNode);
    }

    /// <summary>
    /// 将等待队列中的一个节点移动到服务队列的末尾
    /// </summary>
    /// <param name="waitingNode"></param>
    private void MoveToWorkingQueue(LinkedListNode<AirConditionerService> waitingNode)
    {
        AirConditionerState state = States[waitingNode.Value.Room.Id];
        state.Status = AirConditionerStatus.Working;

        AirConditionerService service = waitingNode.Value;
        service.BeginTemperature = state.CurrentTemperature;
        service.BeginTime = TimeService.Now;

        _serviceQueue.AddLast(service);
        _waitingQueue.Remove(waitingNode);
    }
}
