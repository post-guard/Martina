using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Martina.Services;

/// <summary>
/// 用户服务
/// </summary>
public sealed class UserService(
    MartinaDbContext dbContext,
    IOptions<JsonWebTokenOption> jsonWebTokenOption,
    ILogger<UserService> logger) : IDisposable
{
    private readonly HMACSHA256 _hash256Calculator = new(Encoding.UTF8.GetBytes(jsonWebTokenOption.Value.PasswordKey));

    private readonly JsonWebTokenOption _option = jsonWebTokenOption.Value;

    private readonly SigningCredentials _signingCredentials =
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jsonWebTokenOption.Value.JsonWebTokenKey)),
            SecurityAlgorithms.HmacSha256);

    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

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
            UserId = request.UserId, Username = request.Username, Password = CalculatePasswordHash(request.Password)
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

        if (user.Password == CalculatePasswordHash(request.Password))
        {
            logger.LogDebug("User {} login.", request.UserId);
            return GenerateJsonWebToken(user);
        }

        throw new UserException("Wrong password");
    }

    public void Dispose()
    {
        _hash256Calculator.Dispose();
    }

    private string CalculatePasswordHash(string password)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);

        for (int i = 0; i < _option.HashCount; i++)
        {
            bytes = _hash256Calculator.ComputeHash(bytes);
        }

        return Convert.ToHexString(bytes);
    }

    private string GenerateJsonWebToken(User user)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, user.Username)
        ];

        JwtSecurityToken token = new(
            issuer: _option.Issuer,
            audience: user.UserId,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddDays(7),
            claims: claims,
            signingCredentials: _signingCredentials
        );

        return _jwtSecurityTokenHandler.WriteToken(token);
    }
}
