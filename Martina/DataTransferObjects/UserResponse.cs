using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Martina.Entities;

namespace Martina.DataTransferObjects;

/// <summary>
/// 用户信息传输类
/// </summary>
public class UserResponse
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [JsonPropertyName("name")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 用户的权限
    /// </summary>
    [Required]
    [JsonPropertyName("auth")]
    public PermissionResponse Permission { get; set; } = new();

    public UserResponse()
    {

    }

    public UserResponse(User user)
    {
        UserId = user.UserId;
        Username = user.Username;

        Permission = new PermissionResponse
        {
            Administrator = user.Permission.IsAdministrator,
            AirConditionorAdministrator = user.Permission.AirConditionorAdministrator,
            RoomAdministrator = user.Permission.RoomAdministrator,
            BillAdministrator = user.Permission.BillAdminstrator
        };
    }
}
