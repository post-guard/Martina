using Martina.Entities;
using Martina.Enums;
using MongoDB.Bson;

namespace Martina.Models;

public class AirConditionerState(Room room, bool cooling)
{
    public ObjectId RoomId { get; } = room.Id;

    public string RoomName { get; } = room.RoomName;

    public bool Cooling { get; } = cooling;

    public float CurrentTemperature { get; set; } = room.RoomBasicTemperature;

    public float BasicTemperature { get; set; } = room.RoomBasicTemperature;

    public float TargetTemperature { get; set; } = room.RoomBasicTemperature;

    public FanSpeed Speed { get; set; } = FanSpeed.Low;

    public bool Openning { get; set; } = false;
}
