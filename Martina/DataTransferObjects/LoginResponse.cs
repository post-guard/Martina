using System.ComponentModel.DataAnnotations;

namespace Martina.DataTransferObjects;

/// <summary>
/// 用户登录信息的传输类
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 登录成功之后生成的令牌
    /// </summary>
    [Required]
    public required string Token { get; set; }
}
