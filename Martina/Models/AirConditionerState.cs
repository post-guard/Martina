using Martina.Entities;
using Martina.Enums;
using MongoDB.Bson;

namespace Martina.Models;

public class AirConditionerState(Room room, AirConditionerOption option)
{
    public Room Room { get; } = room;

    public bool Cooling { get; } = option.Cooling;

    public decimal CurrentTemperature { get; set; } = room.RoomBasicTemperature;

    public decimal TargetTemperature { get; set; } = option.DefaultTemperature;

    public FanSpeed Speed { get; set; } = option.DefaultFanSpeed;

    public AirConditionerStatus Status { get; set; } = AirConditionerStatus.Closed;

    public bool OnTarget => Cooling ? CurrentTemperature <= TargetTemperature : CurrentTemperature >= TargetTemperature;
}
