namespace Martina.Enums;

/// <summary>
/// 用户权限枚举
/// </summary>
[Flags]
public enum Roles
{
    User = 0b_0000_0000,
    RoomAdministrator = 0b_0000_0001,
    AirConditionerAdministrator = 0b_0000_0010,
    BillAdministrator = 0b_0000_0100,
    Administrator = 0b_0000_1000
}
