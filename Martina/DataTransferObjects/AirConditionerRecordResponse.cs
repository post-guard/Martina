using System.ComponentModel.DataAnnotations;
using Martina.Entities;

namespace Martina.DataTransferObjects;

public class AirConditionerRecordResponse
{
    /// <summary>
    /// 开始使用的时间
    /// </summary>
    [Required]
    public long BeginTime { get; set; }

    /// <summary>
    /// 结束使用的时间
    /// </summary>
    [Required]
    public long EndTime { get; set; }

    /// <summary>
    /// 开始使用时的温度
    /// </summary>
    [Required]
    public decimal BeginTemperature { get; set; }

    /// <summary>
    /// 结束使用时的温度
    /// </summary>
    [Required]
    public decimal EndTemperature { get; set; }

    /// <summary>
    /// 使用空调造成的温度变化量
    /// </summary>
    [Required]
    public decimal TemperatureDelta { get; set; }

    /// <summary>
    /// 当前空调的费用
    /// 单位是元每度
    /// </summary>
    [Required]
    public decimal Price { get; set; }

    /// <summary>
    /// 使用空调的费用
    /// </summary>
    [Required]
    public decimal Fee { get; set; }

    /// <summary>
    /// 是否结账
    /// </summary>
    [Required]
    public bool Checked { get; set; }

    public AirConditionerRecordResponse()
    {

    }

    public AirConditionerRecordResponse(AirConditionerRecord airConditionerRecord)
    {
        BeginTime = airConditionerRecord.BeginTime.ToUnixTimeSeconds();
        EndTime = airConditionerRecord.EndTime.ToUnixTimeSeconds();
        BeginTemperature = airConditionerRecord.BeginTemperature;
        EndTemperature = airConditionerRecord.EndTemperature;
        TemperatureDelta = decimal.Abs(BeginTemperature - EndTemperature);
        Price = airConditionerRecord.Price;
        Fee = airConditionerRecord.Fee;
        Checked = airConditionerRecord.Checked;
    }
}
