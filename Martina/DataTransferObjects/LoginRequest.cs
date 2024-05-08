using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Martina.DataTransferObjects;

/// <summary>
/// 用户登录请求的传输类
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 需要登录的用户ID
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public required string UserId { get; set; }

    /// <summary>
    /// 登录用户的密码
    /// </summary>
    [Required]
    public required string Password { get; set; }
}
