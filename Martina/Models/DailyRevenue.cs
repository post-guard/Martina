using System.ComponentModel.DataAnnotations;

namespace Martina.Models;

public class DailyRevenue
{
    /// <summary>
    /// 当天的时间
    /// </summary>
    [Required]
    public DateTimeOffset Day { get; set; }

    /// <summary>
    /// 当天的房费收入
    /// </summary>
    [Required]
    public decimal RoomRevenue { get; set; }

    /// <summary>
    /// 当天的空调收入
    /// </summary>
    [Required]
    public decimal AirConditionerRevenue { get; set; }
}
