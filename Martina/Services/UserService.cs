using Martina.DataTransferObjects;
using Martina.Exceptions;
using Martina.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

/// <summary>
/// 用户服务
/// </summary>
public sealed class UserService(
    MartinaDbContext dbContext,
    SecretsService secretsService,
    ILogger<UserService> logger)
{
    /// <summary>
    /// 注册新的用户
    /// </summary>
    /// <param name="request">注册用户请求</param>
    /// <exception cref="UserException">注册用户异常</exception>
    public async Task Register(RegisterRequest request)
    {
        IQueryable<User> existedUsers = from item in dbContext.Users
                                        where item.UserId == request.UserId
                                        select item;

        if (await existedUsers.AnyAsync())
        {
            throw new UserException("User with the same userId has been existed");
        }

        User user = new()
        {
            UserId = request.UserId,
            Username = request.Username,
            Password = await secretsService.CalculatePasswordHash(request.Password)
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("New user '{}' has been registered.", request.UserId);
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">用户登录请求</param>
    /// <returns>登录成功的JWT令牌</returns>
    /// <exception cref="UserException">登录中的用户异常</exception>
    public async Task<string> Login(LoginRequest request)
    {
        IQueryable<User> existedUsers = from item in dbContext.Users
                                        where item.UserId == request.UserId
                                        select item;

        User? user = await existedUsers.FirstOrDefaultAsync();
        if (user is null)
        {
            throw new UserException("Target user does not exist");
        }

        if (user.Password == await secretsService.CalculatePasswordHash(request.Password))
        {
            logger.LogDebug("User {} login.", request.UserId);
            return secretsService.GenerateJsonWebToken(user);
        }

        throw new UserException("Wrong password");
    }

    /// <summary>
    /// 更新用户的信息
    /// </summary>
    /// <param name="userResponse">更新用户信息的请求</param>
    /// <exception cref="UserException">更新过程中的异常</exception>
    public async Task UpdateUserInformation(UserResponse userResponse)
    {
        User? user = await dbContext.Users
            .Where(u => u.UserId == userResponse.UserId)
            .Include(u => u.Permission)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            throw new UserException("Target user is not existed.");
        }

        UserPermission permission = user.Permission;

        if (permission.IsAdministrator)
        {
            throw new UserException("Can't update information of administrator.");
        }

        user.Username = userResponse.Username;

        permission.RoomAdministrator = userResponse.Permission.RoomAdministrator;
        permission.AirConditionorAdministrator = userResponse.Permission.AirConditionerAdministrator;
        permission.BillAdminstrator = userResponse.Permission.BillAdministrator;

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 创建用户
    /// 默认密码为用户ID
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="username">用户名</param>
    /// <returns></returns>
    public async Task<User> CreateUser(string userId, string username)
    {
        IQueryable<User> existedQuery = from item in dbContext.Users.AsNoTracking()
            where item.UserId == userId
            select item;

        User? existedUser = await existedQuery.FirstOrDefaultAsync();
        if (existedUser is not null)
        {
            return existedUser;
        }

        User newUser = new()
        {
            Id = ObjectId.GenerateNewId(),
            UserId = userId,
            Username = username,
            Password = await secretsService.CalculatePasswordHash(userId)
        };

        await dbContext.Users.AddAsync(newUser);
        await dbContext.SaveChangesAsync();

        return newUser;
    }
}
