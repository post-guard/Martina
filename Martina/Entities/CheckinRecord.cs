using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Martina.Entities;

public class CheckinRecord
{
    public ObjectId Id { get; set; }

    public ObjectId RoomId { get; set; }

    [MaxLength(20)]
    public string UserId { get; set; } = string.Empty;

    public DateTimeOffset BeginTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public bool Checkout { get; set; }
}
