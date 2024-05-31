using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Exceptions;
using Martina.Tests.Fixtures;
using Martina.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Moq;

namespace Martina.Tests.Services;

public class CheckinServiceTests(DatabaseFixture databaseFixture) : IClassFixture<DatabaseFixture>
{
    private readonly IOptions<JsonWebTokenOption> _jsonWebOTokenOption =
        MockCreater.CreateJsonWebTokenOptionMock();

    private readonly ILogger<UserService> _logger = new Mock<ILogger<UserService>>().Object;

    [Fact]
    public async Task CreateUserWhenCheckinTest()
    {
        await using MartinaDbContext dbContext = databaseFixture.CreateDbContext();
        TimeService timeService = new(MockCreater.CreateTimeOptionMock());
        await timeService.StartAsync(CancellationToken.None);

        UserService userService = new(dbContext, new SecretsService(_jsonWebOTokenOption), _logger);
        CheckinService checkinService = new(dbContext, userService);
        ISchedular schedular = MockCreater.CreateSchedularMock(dbContext, new AirConditionerOption());
        RoomService roomService = new(dbContext, schedular);
        RoomController roomController = new(roomService);

        IActionResult result = await roomController.CreateRoom(new CreateRoomRequest
        {
            RoomName = "101", RoomBasicTemperature = 25, PricePerDay = 125
        });
        CreatedResult createdResult = Assert.IsType<CreatedResult>(result);
        RoomResponse roomResponse = Assert.IsType<RoomResponse>(createdResult.Value);

        await checkinService.Checkin(new CheckinRequest
        {
            RoomId = roomResponse.RoomId,
            UserId = "1",
            Username = "test",
            BeginTime = DateTimeOffset.Now.AddHours(-6).ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds()
        });

        Assert.Contains(dbContext.Users, u => u is { UserId: "1", Username: "test" });

        CheckinRecord? checkinRecord = await roomService.QueryRoomCurrentStatus(new ObjectId(roomResponse.RoomId));
        Assert.NotNull(checkinRecord);

        Assert.Equal("1", checkinRecord.UserId);

        await timeService.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CheckinConflictTest()
    {
        await using MartinaDbContext dbContext = databaseFixture.CreateDbContext();

        UserService userService = new(dbContext, new SecretsService(_jsonWebOTokenOption), _logger);
        CheckinService checkinService = new(dbContext, userService);
        ISchedular schedular = MockCreater.CreateSchedularMock(dbContext, new AirConditionerOption());
        RoomService roomService = new(dbContext, schedular);
        RoomController roomController = new(roomService);

        IActionResult result = await roomController.CreateRoom(new CreateRoomRequest
        {
            RoomName = "102", RoomBasicTemperature = 25, PricePerDay = 125
        });
        CreatedResult createdResult = Assert.IsType<CreatedResult>(result);
        RoomResponse roomResponse = Assert.IsType<RoomResponse>(createdResult.Value);

        await checkinService.Checkin(new CheckinRequest
        {
            RoomId = roomResponse.RoomId,
            UserId = "1",
            Username = "test",
            BeginTime = DateTimeOffset.Now.AddHours(-6).ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds()
        });

        await checkinService.Checkin(new CheckinRequest
        {
            RoomId = roomResponse.RoomId,
            UserId = "1",
            Username = "test",
            BeginTime = 0,
            EndTime = 2
        });

        await Assert.ThrowsAsync<CheckinException>(async () =>
        {
            await checkinService.Checkin(new CheckinRequest
            {
                RoomId = roomResponse.RoomId,
                UserId = "1",
                Username = "test",
                BeginTime = DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.Now.ToUnixTimeSeconds()
            });
        });

        await Assert.ThrowsAsync<CheckinException>(async () =>
        {
            await checkinService.Checkin(new CheckinRequest
            {
                RoomId = roomResponse.RoomId,
                UserId = "1",
                Username = "test",
                BeginTime = DateTimeOffset.Now.AddHours(12).ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.Now.AddHours(36).ToUnixTimeSeconds()
            });
        });
    }

    [Fact]
    public async Task CheckinTimeTest()
    {
        await using MartinaDbContext dbContext = databaseFixture.CreateDbContext();
        CheckinService checkinService = ServiceCreator.CreateCheckinService(dbContext);

        Room room = new()
        {
            Id = ObjectId.GenerateNewId(), RoomName = "123123", Price = 100, RoomBasicTemperature = 25
        };
        await dbContext.Rooms.AddAsync(room);
        await dbContext.SaveChangesAsync();

        DateTimeOffset start = new(new DateTime(2024, 5, 30));
        Assert.Equal(start, start.ToLocalTime());
        DateTimeOffset end = start.AddDays(1);

        CheckinRecord record = await checkinService.Checkin(new CheckinRequest
        {
            RoomId = room.Id.ToString(),
            Username = "test",
            UserId = "test",
            BeginTime = 1717027200,
            EndTime = 1717113600
        });

        Assert.Equal(start, record.BeginTime);
        Assert.Equal(end, record.EndTime);
    }

    [Fact]
    public async Task CheckoutTest()
    {
        await using MartinaDbContext dbContext = databaseFixture.CreateDbContext();
        CheckinService checkinService = ServiceCreator.CreateCheckinService(dbContext);
        BillService billService = ServiceCreator.CreateBillService(dbContext);

        Room room = new()
        {
            Id = ObjectId.GenerateNewId(), RoomName = "123123", Price = 100, RoomBasicTemperature = 25
        };
        await dbContext.Rooms.AddAsync(room);
        await dbContext.SaveChangesAsync();

        CheckinRecord record = await checkinService.Checkin(new CheckinRequest
        {
            RoomId = room.Id.ToString(),
            Username = "测试",
            UserId = "test",
            BeginTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds()
        });

        await billService.Checkout([record.Id]);

        await checkinService.Checkin(new CheckinRequest
        {
            RoomId = room.Id.ToString(),
            Username = "测试",
            UserId = "test",
            BeginTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds()
        });
    }
}
