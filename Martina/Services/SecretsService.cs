using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Martina.Entities;
using Martina.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Martina.Services;

public sealed class SecretsService(IOptions<JsonWebTokenOption> jsonWebTokenOption) : IDisposable
{
    private readonly JsonWebTokenOption _option = jsonWebTokenOption.Value;

    private readonly HMACSHA256 _hash256Calculator = new(Encoding.UTF8.GetBytes(jsonWebTokenOption.Value.PasswordKey));

    private readonly SigningCredentials _signingCredentials =
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jsonWebTokenOption.Value.JsonWebTokenKey)),
            SecurityAlgorithms.HmacSha256);

    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

    /// <summary>
    /// 生成对应用户的JWT令牌
    /// </summary>
    /// <param name="user">需要生成令牌的用户</param>
    /// <returns>生成的令牌</returns>
    public string GenerateJsonWebToken(User user)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId)
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

    /// <summary>
    /// 计算密码的哈希值
    /// </summary>
    /// <param name="password">需要计算的密码</param>
    /// <returns>计算之后的哈希值</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> CalculatePasswordHash(string password)
    {
        await using Stream stream = new MemoryStream();
        await stream.WriteAsync(Encoding.UTF8.GetBytes(password));

        for (int i = 0; i < _option.HashCount; i++)
        {
            stream.Position = 0;
            byte[] hash = await _hash256Calculator.ComputeHashAsync(stream);
            stream.Position = 0;
            await stream.WriteAsync(hash);
        }

        stream.Position = 0;
        byte[] result = new byte[32];
        int length = await stream.ReadAsync(result);
        if (length != 32)
        {
            throw new InvalidOperationException();
        }

        return Convert.ToHexString(result);
    }

    public void Dispose()
    {
        _hash256Calculator.Dispose();
    }
}
