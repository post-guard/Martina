namespace Martina.Tests.Utils;

public static class ServiceCreator
{
    public static UserService CreateUserService(MartinaDbContext dbContext)
    {
        return new UserService(dbContext, new SecretsService(MockCreater.CreateJsonWebTokenOptionMock()),
                    MockCreater.CreateLoggerMock<UserService>());
    }

    public static CheckinService CreateCheckinService(MartinaDbContext dbContext)
    {
        return new CheckinService(dbContext, CreateUserService(dbContext));
    }

    public static BillService CreateBillService(MartinaDbContext dbContext)
    {
        return new BillService(dbContext, CreateCheckinService(dbContext));
    }
}
