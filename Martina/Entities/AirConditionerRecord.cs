using Martina.Enums;
using MongoDB.Bson;

namespace Martina.Entities;

/// <summary>
/// 空调使用详单记录实体类
/// </summary>
public class AirConditionerRecord
{
    public ObjectId Id { get; set; }

    public ObjectId RoomId { get; set; }

    public DateTimeOffset BeginTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public FanSpeed Speed { get; set; }

    public decimal BeginTemperature { get; set; }

    public decimal EndTemperature { get; set; }

    public decimal Price { get; set; }

    public decimal Fee { get; set; }

    public bool Checked { get; set; }
}
