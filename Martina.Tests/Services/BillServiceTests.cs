using Martina.DataTransferObjects;
using Martina.Enums;
using Martina.Tests.Fixtures;
using Martina.Tests.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Moq;

namespace Martina.Tests.Services;

public class BillServiceTests(DatabaseFixture databaseFixture) : IClassFixture<DatabaseFixture>
{
    private readonly IOptions<JsonWebTokenOption> _jsonWebOTokenOption =
        MockCreater.CreateJsonWebTokenOptionMock();

    private readonly ILogger<UserService> _logger = new Mock<ILogger<UserService>>().Object;

    [Fact]
    public async Task QueryTest()
    {
        await using MartinaDbContext dbContext = databaseFixture.CreateDbContext();
        TimeService timeService = new(MockCreater.CreateTimeOptionMock());
        await timeService.StartAsync(CancellationToken.None);

        Room room = new() { Id = ObjectId.GenerateNewId(), RoomName = "101", RoomBasicTemperature = 25, Price = 125 };
        await dbContext.Rooms.AddAsync(room);
        await dbContext.SaveChangesAsync();

        UserService userService = new(dbContext, new SecretsService(_jsonWebOTokenOption), _logger);
        CheckinService checkinService = new(dbContext, userService);
        BillService billService = new(dbContext, checkinService);

        CheckinRecord checkinRecord = await checkinService.Checkin(new CheckinRequest
        {
            RoomId = room.Id.ToString(),
            UserId = "test",
            Username = "test",
            BeginTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds()
        });

        Assert.Contains(dbContext.CheckinRecords, r => r.RoomId == room.Id);

        BillRecord record = await billService.GenerateBillRecord([checkinRecord.Id]);

        Assert.Equal(125, record.RoomFee);
        Assert.Equal(0, record.AirConditionerFee);

        await timeService.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CalulateRoomFeeTest()
    {
        await using MartinaDbContext dbContext = databaseFixture.CreateDbContext();
        TimeService timeService = new(MockCreater.CreateTimeOptionMock());
        await timeService.StartAsync(CancellationToken.None);

        Room room1 = new() { Id = ObjectId.GenerateNewId(), RoomName = "101", RoomBasicTemperature = 25, Price = 125 };
        Room room2 = new() { Id = ObjectId.GenerateNewId(), RoomName = "102", Price = 1000, RoomBasicTemperature = 25 };
        await dbContext.Rooms.AddAsync(room1);
        await dbContext.Rooms.AddAsync(room2);
        await dbContext.SaveChangesAsync();

        UserService userService = new(dbContext, new SecretsService(_jsonWebOTokenOption), _logger);
        CheckinService checkinService = new(dbContext, userService);
        BillService billService = new(dbContext, checkinService);

        CheckinRecord checkinRecord1 = await checkinService.Checkin(new CheckinRequest
        {
            RoomId = room1.Id.ToString(),
            UserId = "test",
            Username = "test",
            BeginTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds()
        });

        CheckinRecord checkinRecord2 = await checkinService.Checkin(new CheckinRequest
        {
            RoomId = room2.Id.ToString(),
            UserId = "test",
            Username = "test",
            BeginTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.Now.AddDays(3).ToUnixTimeSeconds()
        });

        await dbContext.AirConditionerRecords.AddAsync(new AirConditionerRecord
        {
            Id = ObjectId.GenerateNewId(),
            RoomId = room1.Id,
            BeginTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddMinutes(3),
            BeginTemperature = 26,
            EndTemperature = 24,
            Price = 1,
            Fee = 3,
            Speed = FanSpeed.Middle
        });
        await dbContext.SaveChangesAsync();

        BillRecord record = await billService.GenerateBillRecord([checkinRecord1.Id, checkinRecord2.Id]);

        Assert.Equal(3125, record.RoomFee);
        Assert.Equal(3, record.AirConditionerFee);

        await timeService.StopAsync(CancellationToken.None);
    }
}
