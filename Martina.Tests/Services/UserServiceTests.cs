using Martina.DataTransferObjects;
using Martina.Tests.Fixtures;
using Martina.Tests.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Martina.Tests.Services;

public class UserServiceTests(DatabaseFixture databaseFixture) : IClassFixture<DatabaseFixture>
{
    private readonly IOptions<JsonWebTokenOption> _jsonWebOTokenOption = MockCreater.CreateJsonWebTokenOptionMock();

    private readonly ILogger<UserService> _logger = new Mock<ILogger<UserService>>().Object;

    [Fact]
    public async Task RegisterTest()
    {
        await using MartinaDbContext context = databaseFixture.CreateDbContext();

        UserService userService = new(context, new SecretsService(_jsonWebOTokenOption), _logger);

        await userService.CreateUser("test", "test");

        Assert.Contains(context.Users, u => u.Username == "test");
    }

    [Fact]
    public async Task LoginTest()
    {
        await using MartinaDbContext context = databaseFixture.CreateDbContext();

        UserService userService = new(context, new SecretsService(_jsonWebOTokenOption), _logger);

        await userService.CreateUser("login-test", "login-test");
        await userService.Login(new LoginRequest
        {
            UserId = "login-test",
            Password = "login-test"
        });
    }
}
