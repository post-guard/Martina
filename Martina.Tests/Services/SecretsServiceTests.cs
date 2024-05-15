using Martina.Tests.Utils;
using Microsoft.Extensions.Options;
using Moq;

namespace Martina.Tests.Services;

public class SecretsServiceTests
{
    private readonly Mock<IOptions<JsonWebTokenOption>> _optionMock = MockCreater.CreateJsonWebTokenOptionMock();

    [Fact]
    public async Task CalculatePasswordHashTest()
    {
        SecretsService service = new(_optionMock.Object);

        Assert.Equal("DFDBCBA46DBF33DDB6ADB0912F6EB9D0254B4E401A062685058E19BB29605E43",
            await service.CalculatePasswordHash("12345678"));

        Assert.NotEqual("DFDBCBA46DBF33DDB6ADB0912F6EB9D0254B4E401A062685058E19BB29605E43",
                    await service.CalculatePasswordHash("1234567890"));
    }
}
