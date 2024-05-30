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
        await using MartinaDbContext dbContext = fixture.CreateDbContext();

        List<CheckinRecord> records = await TestCase(
            new AirConditionerOption
            {
                Cooling = false, TemperatureThreshold = decimal.One / 2, BackSpeed = decimal.One / 2
            }, AirConditionerTestCases.HotRooms, AirConditionerTestCases.HotCheckinRecords,
            AirConditionerTestCases.HotCases);

        Assert.Equal(5, records.Count);

        CheckinService checkinService = ServiceCreator.CreateCheckinService(dbContext);
        BillService billService = ServiceCreator.CreateBillService(dbContext);
        BillRecord bill = await billService.GenerateBillRecord(records.Select(r => r.Id));

        List<List<AirConditionerRecord>> airConditionerFees = records
            .Select(r =>
                checkinService.QueryAirConditionerRecords(r.RoomId, r.BeginTime, r.EndTime))
            .Select(r => r.ToList())
            .ToList();

        List<double> exceptFees = [17, 12, 9.33, 12, 8];
        Assert.True(exceptFees.Zip(airConditionerFees.Select(l => l.Select(r => r.Fee).Sum()))
            .Select(p => NumberEqual(p.First, p.Second))
            .All(r => r));

        Assert.True(NumberEqual(58.33, bill.AirConditionerFee));
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


    private async Task<List<CheckinRecord>> TestCase(AirConditionerOption option, List<(string, decimal)> testRooms,
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
            MockCreater.CreateOutputLoggerMock<BuptSchedular>(outputHelper));

        await schedular.StartAsync(CancellationToken.None);

        TimeService timeService = new(timeOptinMock);
        await timeService.StartAsync(CancellationToken.None);

        RoomService roomService = new(dbContext, schedular);
        CheckinService checkinService = ServiceCreator.CreateCheckinService(dbContext);
        AirConditionerTestService airConditionerTestService = new(roomService, checkinService, schedular, dbContext,
            timeOptinMock, MockCreater.CreateOutputLoggerMock<AirConditionerTestService>(outputHelper));

        Dictionary<string, Room> rooms =
            await airConditionerTestService.CreateTestRoom(testRooms);
        List<CheckinRecord> records = await airConditionerTestService.CreateCheckinRecords(rooms, testCheckinRecords);
        await airConditionerTestService.SendAirConditionerRequests(rooms, testAirConditionerRequests);

        await schedular.StopAsync(CancellationToken.None);
        await timeService.StopAsync(CancellationToken.None);
        return records;
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

    private static bool NumberEqual(double except, decimal actual)
    {
        return decimal.Abs(new decimal(except) - actual) < decimal.One / 100;
    }
}
