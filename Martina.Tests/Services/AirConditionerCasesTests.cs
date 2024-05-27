using Martina.DataTransferObjects;
using Martina.Tests.Fixtures;
using Martina.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Martina.Tests.Services;

public class AirConditionerCasesTests(DatabaseFixture fixture, ITestOutputHelper outputHelper)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task HotCaseTest()
    {
        await TestCase(
            new AirConditionerOption
            {
                Cooling = false, TemperatureThreshold = decimal.One / 2, BackSpeed = decimal.One / 2
            }, AirConditionerTestCases.HotRooms, AirConditionerTestCases.HotCheckinRecords,
            AirConditionerTestCases.HotCases);
    }

    [Fact]
    public async Task CoolCaseTest()
    {
        await TestCase(
            new AirConditionerOption
            {
                Cooling = true, TemperatureThreshold = decimal.One / 2, BackSpeed = decimal.One / 2
            }, AirConditionerTestCases.CoolRooms, AirConditionerTestCases.CoolCheckinRecords,
            AirConditionerTestCases.CoolCases);
    }


    private async Task TestCase(AirConditionerOption option, List<(string, decimal)> testRooms,
        List<(string, int)> testCheckinRecords,
        List<Dictionary<string, AirConditionerRequest>> testAirConditionerRequests)
    {
        ServiceCollection serviceCollection = [];
        IOptions<TimeOption> timeOptinMock = MockCreater.CreateTimeOptionMock();
        await using MartinaDbContext dbContext = fixture.CreateDbContext();
        serviceCollection.AddScoped<MartinaDbContext>((_) => fixture.CreateDbContext());

        AirConditionerManageService manageService = new() { Opening = true, Option = option };

        BuptSchedular schedular = new(serviceCollection.BuildServiceProvider(), timeOptinMock,
            manageService,
            MockCreater.CreateLoggerMock<BuptSchedular>());

        await schedular.StartAsync(CancellationToken.None);

        RoomService roomService = new(dbContext, schedular);
        UserService userService = new(dbContext, new SecretsService(MockCreater.CreateJsonWebTokenOptionMock()),
            MockCreater.CreateLoggerMock<UserService>());
        CheckinService checkinService = new(dbContext, userService);
        AirConditionerTestService airConditionerTestService = new(roomService, checkinService, schedular, dbContext,
            timeOptinMock, MockCreater.CreateOutputLoggerMock<AirConditionerTestService>(outputHelper));

        Dictionary<string, Room> rooms =
            await airConditionerTestService.CreateTestRoom(testRooms);
        await airConditionerTestService.CreateCheckinRecords(rooms, testCheckinRecords);
        await airConditionerTestService.SendAirConditionerRequests(rooms, testAirConditionerRequests);

        await schedular.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void LinkedListTest()
    {
        LinkedList<int> list = [];

        list.AddLast(1);
        list.AddLast(2);
        list.AddLast(3);

        Assert.Equal([1, 2, 3], list.ToList());
    }
}
