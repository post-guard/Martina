using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Tests.Fixtures;
using Martina.Tests.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Martina.Tests.Controllers;

public class RoomControllerTests(DatabaseFixture databaseFixture) : IClassFixture<DatabaseFixture>
{
    [Theory]
    [InlineData("101", 100, 25)]
    [InlineData("102", 120, 23)]
    public async Task CreateRoomTest(string roomName, decimal price, decimal temperature)
    {
        await using MartinaDbContext context = databaseFixture.CreateDbContext();
        ISchedular schedular = MockCreater.CreateSchedularMock(context, new AirConditionerOption());

        RoomService roomService = new(context, schedular);
        RoomController controller = new(roomService);

        await controller.CreateRoom(new CreateRoomRequest
        {
            RoomName = roomName, PricePerDay = price, RoomBasicTemperature = temperature
        });

        Assert.Contains(context.Rooms, r =>
            r.RoomName == roomName && r.RoomBasicTemperature == temperature &&
            r.Price == price);

        IActionResult result = await controller.ListAllRooms();
        OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(result);
        List<RoomResponse> rooms = Assert.IsAssignableFrom<IEnumerable<RoomResponse>>(okObjectResult.Value).ToList();

        Assert.Contains(rooms, r =>
            r.RoomName == roomName && r.RoomBaiscTemperature == temperature &&
            r.PricePerDay == price);

        foreach (RoomResponse room in rooms)
        {
            result = await controller.GetRoom(room.RoomId);
            Assert.IsType<OkObjectResult>(result);
        }
    }

    [Fact]
    public async Task NotFountTest()
    {
        await using MartinaDbContext context = databaseFixture.CreateDbContext();
        ISchedular schedular = MockCreater.CreateSchedularMock(context, new AirConditionerOption());

        RoomService roomService = new(context, schedular);
        RoomController controller = new(roomService);

        IActionResult result = await controller.GetRoom("asdasdasd");
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
