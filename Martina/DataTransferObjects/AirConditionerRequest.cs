using System.ComponentModel.DataAnnotations;
using Martina.Enums;

namespace Martina.DataTransferObjects;

public class AirConditionerRequest
{
    /// <summary>
    /// 开启还是关闭空调
    /// </summary>
    [Required]
    public bool Open { get; set; }

    /// <summary>
    /// 要求的目标的温度
    /// </summary>
    public decimal TargetTemperature { get; set; }

    /// <summary>
    /// 要求的目标风速
    /// </summary>
    public FanSpeed Speed { get; set; }

    public override string ToString()
        => $"Request for air conditioner: {Open}-{TargetTemperature}-{Speed}";
}
