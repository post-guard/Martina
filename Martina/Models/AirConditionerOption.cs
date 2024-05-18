namespace Martina.Models;

public class AirConditionerOption
{
    public decimal MinTemperature { get; set; }

    public decimal MaxTemperature { get; set; }

    public decimal DefaultTemperature { get; set; }

    public decimal DefaultFanSpeed { get; set; }

    public decimal HighSpeedPerDegree { get; set; } = decimal.One;

    public decimal MiddleSpeedPerDegree { get; set; } = 2;

    public decimal LowSpeedPerDegree { get; set; } = 3;

    public decimal BackSpeed { get; set; } = 1;
}
