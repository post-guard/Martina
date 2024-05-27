using System.ComponentModel.DataAnnotations;
using Martina.Enums;

namespace Martina.Models;

public class AirConditionerOption
{
    /// <summary>
    /// 当前空调系统是制冷还是制热
    /// </summary>
    [Required]
    public bool Cooling { get; set; } = true;

    /// <summary>
    /// 当前空调系统的最低温度
    /// </summary>
    [Required]
    public decimal MinTemperature { get; set; }

    /// <summary>
    /// 当前空调系统的最高温度
    /// </summary>
    [Required]
    public decimal MaxTemperature { get; set; }

    /// <summary>
    /// 当前空调系统的默认温度
    /// </summary>
    [Required]
    public decimal DefaultTemperature { get; set; }

    /// <summary>
    /// 当前空调系统的默认风速
    /// </summary>
    [Required]
    public FanSpeed DefaultFanSpeed { get; set; }

    /// <summary>
    /// 回温之后再次加入工作队列的温差阈值
    /// </summary>
    [Required]
    public decimal TemperatureThreshold { get; set; } = decimal.One;

    /// <summary>
    /// 高速风导致温度变化的速度
    /// 单位为分每度
    /// </summary>
    [Required]
    public decimal HighSpeedPerDegree { get; set; } = decimal.One;

    /// <summary>
    /// 中速风导致温度变化的速度
    /// 单位为分每度
    /// </summary>
    [Required]
    public decimal MiddleSpeedPerDegree { get; set; } = 2;

    /// <summary>
    /// 低速风导致温度变化的速度
    /// 单位为分每度
    /// </summary>
    [Required]
    public decimal LowSpeedPerDegree { get; set; } = 3;

    /// <summary>
    /// 回温的速度
    /// 单位为度每分
    /// </summary>
    [Required]
    public decimal BackSpeed { get; set; } = 1;

    /// <summary>
    /// 每度温度变化的费用
    /// </summary>
    public decimal PricePerDegree { get; set; } = 1;
}
