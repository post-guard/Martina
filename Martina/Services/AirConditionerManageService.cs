using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Models;
using MongoDB.Bson;

namespace Martina.Services;

public class AirConditionerManageService
{
    public bool Opening { get; set; }

    public AirConditionerOption Option { get; set; } = new();

    public ConcurrentDictionary<ObjectId, Room> Rooms { get; } = new();

    public bool VolidateAirConditionerRequest(ObjectId roomId, AirConditionerRequest request,[NotNullWhen(false)] out string? message)
    {
        if (!Opening)
        {
            message = "尚未开启空调系统";
            return false;
        }

        if (request.TargetTemperature < Option.MinTemperature)
        {
            message = $"无法设置为小于{Option.MinTemperature}度";
            return false;
        }

        if (request.TargetTemperature > Option.MaxTemperature)
        {
            message = $"无法设置为低于{Option.MaxTemperature}度";
            return false;
        }

        if (Rooms.TryGetValue(roomId, out Room? room))
        {
            if (Option.Cooling)
            {
                if (room.RoomBasicTemperature >= request.TargetTemperature)
                {
                    message = "无法设置为高于室温的温度";
                    return false;
                }
            }
            else
            {
                if (room.RoomBasicTemperature <= request.TargetTemperature)
                {
                    message = "无法设置为低于室温的温度";
                    return false;
                }
            }
        }

        message = null;
        return true;
    }
}
