using System.ComponentModel.DataAnnotations;
using Martina.Models;

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
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 用户的权限
    /// </summary>
    [Required]
    public PermissionResponse Permission { get; set; } = new();

    public UserResponse()
    {

    }

    public UserResponse(User user, UserPermission permission)
    {
        UserId = user.UserId;
        Username = user.Username;

        Permission = new PermissionResponse
        {
            Administrator = permission.IsAdministrator,
            AirConditionorAdministrator = permission.AirConditionorAdministrator,
            RoomAdministrator = permission.RoomAdministrator,
            BillAdministrator = permission.BillAdminstrator
        };
    }
}
