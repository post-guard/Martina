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
        MockCreater.CreateJsonWebTokenOptionMock().Object;

    private readonly ILogger<UserService> _logger = new Mock<ILogger<UserService>>().Object;

    [Fact]
    public async Task CreateUserWhenCheckinTest()
    {
        await using MartinaDbContext dbContext = databaseFixture.CreateDbContext();

        UserService userService = new(dbContext, new SecretsService(_jsonWebOTokenOption), _logger);
        CheckinService checkinService = new(dbContext, userService);
        RoomService roomService = new(dbContext);
        RoomController roomController = new(dbContext, roomService);

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
    }

    [Fact]
    public async Task CheckinConflictTest()
    {
        await using MartinaDbContext dbContext = databaseFixture.CreateDbContext();

        UserService userService = new(dbContext, new SecretsService(_jsonWebOTokenOption), _logger);
        CheckinService checkinService = new(dbContext, userService);
        RoomService roomService = new(dbContext);
        RoomController roomController = new(dbContext, roomService);

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
}
