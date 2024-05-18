using System.ComponentModel.DataAnnotations;
using Martina.Entities;
using Martina.Models;

namespace Martina.DataTransferObjects;

/// <summary>
/// 房间传输类
/// </summary>
public class RoomResponse
{
    /// <summary>
    /// 房间的ID
    /// </summary>
    [Required]
    public string RoomId { get; set; } = string.Empty;

    /// <summary>
    /// 房间的名称
    /// </summary>
    [Required]
    public string RoomName { get; set; } = string.Empty;

    /// <summary>
    /// 房间的单价
    /// </summary>
    [Required]
    public decimal PricePerDay { get; set; }

    /// <summary>
    /// 房间的基础环境温度
    /// </summary>
    [Required]
    public decimal RoomBaiscTemperature { get; set; }

    /// <summary>
    /// 房间的空调状态
    /// </summary>
    [Required]
    public AirConditionerResponse AirConditioner { get; set; } = new();

    /// <summary>
    /// 房间当前的入住状态
    /// </summary>
    public CheckinResponse? CheckinStatus { get; set; }

    public RoomResponse()
    {

    }

    public RoomResponse(Room room, AirConditionerState state)
    {
        RoomId = room.Id.ToString();
        RoomName = room.RoomName;
        PricePerDay = room.Price;
        RoomBaiscTemperature = room.RoomBasicTemperature;
        AirConditioner = new AirConditionerResponse(state);
    }

    public RoomResponse(Room room, CheckinRecord record, AirConditionerState state) : this(room, state)
    {
        CheckinStatus = new CheckinResponse(record);
    }
}
