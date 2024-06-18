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

    /// <summary>
    /// 验证空调请求的有效性
    /// </summary>
    /// <param name="roomId">发送空调服务请求的房间ID</param>
    /// <param name="request">空调服务请求</param>
    /// <param name="message">如果无效输出提示信息</param>
    /// <returns>是否为有效的空调请求</returns>
    public bool VolidateAirConditionerRequest(ObjectId roomId, AirConditionerRequest request,[NotNullWhen(false)] out string? message)
    {
        if (!Opening)
        {
            message = "尚未开启空调系统";
            return false;
        }

        if (!request.Open)
        {
            // 如果是关机请求就不校验了
            message = null;
            return true;
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
                if (room.RoomBasicTemperature <= request.TargetTemperature)
                {
                    message = "无法设置为高于室温的温度";
                    return false;
                }
            }
            else
            {
                if (room.RoomBasicTemperature >= request.TargetTemperature)
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
