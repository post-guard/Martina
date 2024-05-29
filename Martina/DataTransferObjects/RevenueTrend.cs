using System.ComponentModel.DataAnnotations;
using Martina.Models;

namespace Martina.DataTransferObjects;

public class RevenueTrend
{
    /// <summary>
    /// 酒店当前的总住户数
    /// </summary>
    [Required]
    public int TotalUsers { get; set; }

    /// <summary>
    /// 酒店当前的总入住数
    /// </summary>
    [Required]
    public int TotalCheckin { get; set; }

    /// <summary>
    /// 酒店按天的收入变化
    /// </summary>
    [Required]
    public List<DailyRevenue> DailyRevenues { get; set; } = [];
}
