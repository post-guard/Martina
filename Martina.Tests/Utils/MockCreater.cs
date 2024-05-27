using System.Collections.Concurrent;
using Martina.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Moq;
using Xunit.Abstractions;

namespace Martina.Tests.Utils;

public static class MockCreater
{
    public static IOptions<JsonWebTokenOption> CreateJsonWebTokenOptionMock()
    {
        Mock<IOptions<JsonWebTokenOption>> mock = new();

        mock.SetupGet(o => o.Value)
            .Returns(() => new JsonWebTokenOption
            {
                PasswordKey = "asdfasdf",
                HashCount = 1,
                Issuer = "test",
                JsonWebTokenKey = "asdfasdfasdfasdfasdfasdfasdfasdfasdfasdf"
            });

        return mock.Object;
    }

    public static ISchedular CreateSchedularMock(MartinaDbContext dbContext, AirConditionerOption option)
    {
        Mock<ISchedular> mock = new();

        mock.SetupGet(s => s.States)
            .Returns(() =>
            {
                ConcurrentDictionary<ObjectId, AirConditionerState> states = new();

                foreach (Room room in dbContext.Rooms.AsNoTracking())
                {
                    states.TryAdd(room.Id, new AirConditionerState(room, option));
                }

                return states;
            });

        return mock.Object;
    }

    public static IOptions<TimeOption> CreateTimeOptionMock()
    {
        Mock<IOptions<TimeOption>> mock = new();

        mock.SetupGet(t => t.Value)
            .Returns(new TimeOption { Factor = 30 });

        return mock.Object;
    }

    public static ILogger<T> CreateLoggerMock<T>()
    {
        Mock<ILogger<T>> mock = new();
        return mock.Object;
    }

    public static ILogger<T> CreateOutputLoggerMock<T>(ITestOutputHelper helper)
    {
        ILogger<T> logger = new TestLogger<T>(helper);

        return logger;
    }

    private class TestLogger<T>(ITestOutputHelper testOutputHelper) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            testOutputHelper.WriteLine($"{logLevel}: {formatter(state, exception)}");
        }
    }
}
