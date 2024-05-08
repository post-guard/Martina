using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Martina.DataTransferObjects;

/// <summary>
/// 注册的请求信息传输类
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public required string UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [JsonPropertyName("name")]
    public required string Username { get; set; }

    /// <summary>
    /// 用户密码
    /// </summary>
    [Required]
    public required string Password { get; set; }
}
