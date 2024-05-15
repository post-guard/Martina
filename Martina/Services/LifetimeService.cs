using Martina.Entities;
using Martina.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Martina.Services;

public class LifetimeService(IServiceProvider serviceProvider,
    ILogger<LifetimeService> logger) : IHostedService
{
    private readonly SystemUserOption _option = serviceProvider.GetRequiredService<IOptions<SystemUserOption>>().Value;

    private readonly SecretsService _secretsService = serviceProvider.GetRequiredService<SecretsService>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        MartinaDbContext dbContext = scope.ServiceProvider.GetRequiredService<MartinaDbContext>();

        await EnsureSystemUserCreated(dbContext);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task EnsureSystemUserCreated(MartinaDbContext dbContext)
    {
        IQueryable<User> users = from item in dbContext.Users
                                 where item.UserId == _option.Administrator.UserId
                                 select item;

        User? user = await users.FirstOrDefaultAsync();

        if (user is not null)
        {
            logger.LogInformation("Remove old administrator information.");

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }

        User newUser = new()
        {
            UserId = _option.Administrator.UserId,
            Username = _option.Administrator.Username,
            Password = await _secretsService.CalculatePasswordHash(_option.Administrator.Password),
            Permission = new UserPermission
            {
                IsAdministrator = true
            }
        };

        logger.LogInformation("Create new administrator '{}'.", newUser.Username);

        await dbContext.Users.AddAsync(newUser);

        await dbContext.SaveChangesAsync();
    }
}
