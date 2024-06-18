using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Martina.Entities;

/// <summary>
/// 用户权限实体类
/// </summary>
public class UserPermission
{
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
