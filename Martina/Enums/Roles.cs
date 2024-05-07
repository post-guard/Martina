namespace Martina.Enums;

[Flags]
public enum Roles
{
    User = 0b_0000_0000,
    RoomAdministrator = 0b_0000_0001,
    AirConditionorAdministrator = 0b_0000_0010,
    BillAdministrator = 0b_0000_0100,
    Administrator = 0b_0000_1000
}
