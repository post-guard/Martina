﻿using MongoDB.Bson;

namespace Martina.Entities;

public class BillRecord
{
    public ObjectId Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTimeOffset BeginTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public List<ObjectId> CheckinRecords { get; set; } = [];

    public List<ObjectId> AirConditionerRecords { get; set; } = [];

    public decimal RoomFee { get; set; }

    public decimal AirConditionerFee { get; set; }
}
