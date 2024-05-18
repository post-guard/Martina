using System.ComponentModel.DataAnnotations;
using Martina.Enums;
using Martina.Models;

namespace Martina.DataTransferObjects;

public class AirConditionerResponse
{
    /// <summary>
    /// 空调所在房间的ID
    /// </summary>
    [Required]
    public string RoomId { get; set; } = string.Empty;

    /// <summary>
    /// 空调当前的状态
    /// </summary>
    [Required]
    public AirConditionerStatus Status { get; set; } = AirConditionerStatus.Closed;

    /// <summary>
    /// 房间的当前温度
    /// </summary>
    [Required]
    public decimal Temperature { get; set; }

    /// <summary>
    /// 空调是在制冷还是在制热
    /// </summary>
    [Required]
    public bool Cooling { get; set; }

    /// <summary>
    /// 空调的目标温度
    /// 当空调开启时有效
    /// </summary>
    [Required]
    public decimal TargetTemperature { get; set; }

    /// <summary>
    /// 空调的风速
    /// 当空调开启时有效
    /// </summary>
    [Required]
    public FanSpeed Speed { get; set; }


    public AirConditionerResponse()
    {
    }

    public AirConditionerResponse(AirConditionerState state)
    {
        RoomId = state.Room.Id.ToString();
        Cooling = state.Cooling;
        Status = state.Status;
        Temperature = state.CurrentTemperature;
        TargetTemperature = state.TargetTemperature;
        Speed = state.Speed;
    }
}
