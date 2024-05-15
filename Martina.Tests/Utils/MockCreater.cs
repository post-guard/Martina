using Microsoft.Extensions.Options;
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
}
