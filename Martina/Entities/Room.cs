using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Martina.Entities;

public class Room
{
    public ObjectId Id { get; set; }

    [MaxLength(100)]
    public string RoomName { get; set; } = string.Empty;

    public float Price { get; set; }

    public float RoomBasicTemperature { get; set; }
}
