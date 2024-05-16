using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Martina.DataTransferObjects;

/// <summary>
/// 用户权限信息的传输类
/// </summary>
public class PermissionResponse
{
    /// <summary>
    /// 用户是否为超级管理员
    /// </summary>
    [Required]
    [JsonPropertyName("sudo")]
    public bool Administrator { get; set; }

    /// <summary>
    /// 用户是否为客房管理员
    /// </summary>
    [Required]
    [JsonPropertyName("roomAdmin")]
    public bool RoomAdministrator { get; set; }

    /// <summary>
    /// 用户是否为空调管理员
    /// </summary>
    [Required]
    [JsonPropertyName("airconAdmin")]
    public bool AirConditionerAdministrator { get; set; }

    /// <summary>
    /// 用户是否为账单管理员
    /// </summary>
    [Required]
    [JsonPropertyName("billAdmin")]
    public bool BillAdministrator { get; set; }
}
