using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Martina.Entities;

/// <summary>
/// 酒店房间实体类
/// </summary>
public class Room
{
    public ObjectId Id { get; set; }

    [MaxLength(100)]
    public string RoomName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal RoomBasicTemperature { get; set; }
}
