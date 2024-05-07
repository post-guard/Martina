using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Martina.Models;

public class UserPermission
{
    public ObjectId Id { get; set; }

    [MaxLength(20)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 是否是管理员
    /// </summary>
    public bool IsAdministrator { get; set; }

    /// <summary>
    /// 是否是客房管理员
    /// </summary>
    public bool RoomAdministrator { get; set; }

    /// <summary>
    /// 是否是空调管理员
    /// </summary>
    public bool AirConditionorAdministrator { get; set; }

    /// <summary>
    /// 是否是账单管理员
    /// </summary>
    public bool BillAdminstrator { get; set; }
}
