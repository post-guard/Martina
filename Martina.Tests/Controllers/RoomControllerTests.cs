using Martina.DataTransferObjects;
using Martina.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace Martina.Tests.Controllers;

public class RoomControllerTests(DatabaseFixture databaseFixture) : IClassFixture<DatabaseFixture>
{
    [Theory]
    [InlineData("101", 100, 25)]
    [InlineData("102", 120, 23)]
    public async Task CreateRoomTest(string roomName, float price, float temperature)
    {
        await using MartinaDbContext context = databaseFixture.CreateDbContext();

        RoomService roomService = new(context);
        RoomController controller = new(context, roomService);

        await controller.CreateRoom(new CreateRoomRequest
        {
            RoomName = roomName, PricePerDay = price, RoomBasicTemperature = temperature
        });

        Assert.Contains(context.Rooms, r =>
            r.RoomName == roomName && Math.Abs(r.RoomBasicTemperature - temperature) < 0.01 &&
            Math.Abs(r.Price - price) < 0.01);

        IActionResult result = await controller.ListAllRooms();
        OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(result);
        List<RoomResponse> rooms = Assert.IsAssignableFrom<IEnumerable<RoomResponse>>(okObjectResult.Value).ToList();

        Assert.Contains(rooms, r =>
            r.RoomName == roomName && Math.Abs(r.RoomBaiscTemperature - temperature) < 0.01 &&
            Math.Abs(r.PricePerDay - price) < 0.01);

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

        RoomService roomService = new(context);
        RoomController controller = new(context, roomService);

        IActionResult result = await controller.GetRoom("asdasdasd");
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
