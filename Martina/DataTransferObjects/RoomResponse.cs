using System.ComponentModel.DataAnnotations;
using Martina.Entities;

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
    public float PricePerDay { get; set; }

    /// <summary>
    /// 房间的基础环境温度
    /// </summary>
    [Required]
    public float RoomBaiscTemperature { get; set; }

    public RoomResponse()
    {

    }

    public RoomResponse(Room room)
    {
        RoomId = room.Id.ToString();
        RoomName = room.RoomName;
        PricePerDay = room.Price;
        RoomBaiscTemperature = room.RoomBasicTemperature;
    }
}
