using System.Collections.Concurrent;
using Martina.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Moq;

namespace Martina.Tests.Utils;

public static class MockCreater
{
    public static Mock<IOptions<JsonWebTokenOption>> CreateJsonWebTokenOptionMock()
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

        return mock;
    }

    public static ISchedular CreateSchedularMock(MartinaDbContext dbContext)
    {
        Mock<ISchedular> mock = new();

        mock.SetupGet(s => s.States)
            .Returns(() =>
            {
                ConcurrentDictionary<ObjectId, AirConditionerState> states = new();

                foreach (Room room in dbContext.Rooms.AsNoTracking())
                {
                    states.TryAdd(room.Id, new AirConditionerState(room, true));
                }

                return states;
            });

        return mock.Object;
    }
}
