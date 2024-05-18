using System.ComponentModel.DataAnnotations;

namespace Martina.DataTransferObjects;

public class CreateRoomRequest
{
    /// <summary>
    /// 创建的房间名称
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
    public decimal RoomBasicTemperature { get; set; }
}
