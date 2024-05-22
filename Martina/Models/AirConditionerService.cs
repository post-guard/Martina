using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Enums;

namespace Martina.Models;

public class AirConditionerService(Room room, AirConditionerRequest request)
{
    public Room Room { get; } = room;

    public bool Opening { get; } = request.Open;

    public decimal TargetTemperature { get; } = request.TargetTemperature;

    public FanSpeed Speed { get; } = request.Speed;

    public int TimeToLive { get; set; } = 20;

    /// <summary>
    /// 开始工作时的温度
    /// </summary>
    public decimal BeginTemperature { get; set; }

    /// <summary>
    /// 开始工作时的时间
    /// </summary>
    public DateTimeOffset BeginTime { get; set; }
}
