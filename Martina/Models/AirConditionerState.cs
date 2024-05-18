using Martina.Entities;
using Martina.Enums;
using MongoDB.Bson;

namespace Martina.Models;

public class AirConditionerState(Room room, bool cooling)
{
    public Room Room { get; } = room;

    public bool Cooling { get; } = cooling;

    public decimal CurrentTemperature { get; set; } = room.RoomBasicTemperature;

    public decimal TargetTemperature { get; set; } = room.RoomBasicTemperature;

    public FanSpeed Speed { get; set; } = FanSpeed.Low;

    public AirConditionerStatus Status { get; set; } = AirConditionerStatus.Closed;

    public bool OnTarget => Cooling ? CurrentTemperature <= TargetTemperature : CurrentTemperature >= TargetTemperature;
}
