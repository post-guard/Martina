using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Models;
using Microsoft.Extensions.Options;

namespace Martina.Services;

public class AirConditionerTestService(
    RoomService roomService,
    CheckinService checkinService,
    ISchedular schedular,
    MartinaDbContext dbContext,
    IOptions<TimeOption> timeOption,
    ILogger<AirConditionerTestService> logger)
{
    /// <summary>
    /// 发出空调使用请求
    /// </summary>
    /// <param name="rooms">测试的房间</param>
    /// <param name="testCases">测试集</param>
    /// <param name="cancellationToken">停止测试的取消令牌</param>
    public async Task SendAirConditionerRequests(Dictionary<string, Room> rooms,
        List<Dictionary<string, AirConditionerRequest>> testCases, CancellationToken cancellationToken = default)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds((double)(60 / timeOption.Value.Factor)));
        int i = 0;

        await SendRequest(rooms, testCases[i]);
        i += 1;

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await SendRequest(rooms, testCases[i]);

                Thread.Sleep(TimeSpan.FromMilliseconds((double)(6000 / timeOption.Value.Factor)));

                logger.LogInformation("Time: {}s:", i);
                logger.LogInformation("---------------------------------");
                foreach (Room room in rooms.Values)
                {
                    logger.LogInformation("Room {}: s: {}, t: {}", room.RoomName, schedular.States[room.Id].Status,
                        schedular.States[room.Id].CurrentTemperature);
                }

                i += 1;
                if (i >= testCases.Count)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SendRequest(Dictionary<string, Room> rooms, Dictionary<string, AirConditionerRequest> requests)
    {
        foreach (KeyValuePair<string, AirConditionerRequest> pair in requests)
        {
            await schedular.SendRequest(rooms[pair.Key].Id, pair.Value);
        }
    }

    /// <summary>
    /// 创建测试房间
    /// </summary>
    /// <param name="rooms">需要创建的房间名称和默认温度</param>
    /// <returns>创建好的测试房间</returns>
    public async Task<Dictionary<string, Room>> CreateTestRoom(List<(string, decimal)> rooms)
    {
        IQueryable<Room> existedRoomQuery = from item in dbContext.Rooms
            where rooms.Exists(p => p.Item1 == item.RoomName)
            select item;

        List<Room> existedRooms = existedRoomQuery.ToList();

        if (existedRooms.Count == rooms.Count)
        {
            return existedRooms.ToDictionary(r => r.RoomName);
        }

        // 否则重新创建测试用房间
        dbContext.Rooms.RemoveRange(existedRooms);
        await dbContext.SaveChangesAsync();

        foreach ((string name, decimal temp) in rooms)
        {
            await roomService.CreateRoom(new CreateRoomRequest
            {
                RoomName = name, RoomBasicTemperature = temp, PricePerDay = 100
            });
        }

        return existedRoomQuery.ToDictionary(r => r.RoomName);
    }

    /// <summary>
    /// 在指定的房间中创建入住记录
    /// </summary>
    /// <param name="rooms"></param>
    /// <param name="checkinRecord"></param>
    /// <returns></returns>
    public async Task<List<CheckinRecord>> CreateCheckinRecords(Dictionary<string, Room> rooms,
        List<(string, int)> checkinRecord)
    {
        List<CheckinRecord> result = [];

        foreach ((string roomName, int count) in checkinRecord)
        {
            Room room = rooms[roomName];

            result.Add(await checkinService.Checkin(new CheckinRequest
            {
                RoomId = room.Id.ToString(),
                UserId = roomName,
                Username = "入住测试用户",
                BeginTime = TimeService.Now.ToUnixTimeSeconds(),
                EndTime = TimeService.Now.AddDays(count).ToUnixTimeSeconds()
            }));
        }

        return result;
    }

    /// <summary>
    /// 清除数据库中的指定测试脚本数据
    /// </summary>
    /// <param name="testRooms"></param>
    public async Task ClearTestRecord(List<(string, decimal)> testRooms)
    {
        List<Room> rooms = (from item in dbContext.Rooms
            where testRooms.Exists(p => p.Item1 == item.RoomName)
            select item).ToList();

        IQueryable<CheckinRecord> checkinRecords = from item in dbContext.CheckinRecords
            where rooms.Exists(r => item.RoomId == r.Id)
            select item;

        dbContext.CheckinRecords.RemoveRange(checkinRecords);
        await dbContext.SaveChangesAsync();

        IQueryable<AirConditionerRecord> airConditionerRecords = from item in dbContext.AirConditionerRecords
            where rooms.Exists(r => item.RoomId == r.Id)
            select item;

        dbContext.AirConditionerRecords.RemoveRange(airConditionerRecords);
        await dbContext.SaveChangesAsync();

        dbContext.Rooms.RemoveRange(rooms);
        await dbContext.SaveChangesAsync();

        await schedular.Reset();
    }
}
