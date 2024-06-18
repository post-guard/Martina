using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Exceptions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

public class CheckinService(MartinaDbContext dbContext, UserService userService)
{
    /// <summary>
    /// 查询入住记录列表
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="userId"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public List<CheckinRecord> QueryCheckinRecords(string? roomId, string? userId, long begin, long end)
    {
        DateTimeOffset beginTime = DateTimeOffset.FromUnixTimeSeconds(begin);
        DateTimeOffset endTime = DateTimeOffset.FromUnixTimeSeconds(end);

        IQueryable<CheckinRecord> records = from item in dbContext.CheckinRecords.AsNoTracking()
            where item.BeginTime >= beginTime && item.EndTime <= endTime && !item.Checkout
            select item;

        if (roomId is not null)
        {
            records = from item in records
                where item.RoomId == new ObjectId(roomId)
                select item;
        }

        if (userId is not null)
        {
            records = from item in records
                where item.UserId == userId
                select item;
        }

        return records.ToList();
    }

    /// <summary>
    /// 办理入住
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="CheckinException"></exception>
    public async Task<CheckinRecord> Checkin(CheckinRequest request)
    {
        ObjectId roomId = new(request.RoomId);
        // 前端发送的时间戳是UTC时间
        DateTimeOffset beginTime = DateTimeOffset.FromUnixTimeSeconds(request.BeginTime).AddHours(-8);
        beginTime = TimeZoneInfo.ConvertTime(beginTime, TimeZoneInfo.Local);
        DateTimeOffset endTime = DateTimeOffset.FromUnixTimeSeconds(request.EndTime).AddHours(-8);
        endTime = TimeZoneInfo.ConvertTime(endTime, TimeZoneInfo.Local);

        if (endTime <= beginTime)
        {
            throw new CheckinException("Checkout time must be after of checkin time.");
        }

        User user = await userService.CreateUser(request.UserId, request.Username);

        if (!await dbContext.Rooms.AsNoTracking().AnyAsync(r => r.Id == roomId))
        {
            throw new CheckinException("Checkin room doesn't exist.");
        }

        IQueryable<CheckinRecord> records = from item in dbContext.CheckinRecords.AsNoTracking()
            where item.RoomId == roomId && item.EndTime >= beginTime && item.BeginTime <= endTime && !item.Checkout
            select item;

        if (await records.AnyAsync())
        {
            throw new CheckinException("Checkin room has been booked.");
        }

        CheckinRecord record = new()
        {
            Id = ObjectId.GenerateNewId(),
            RoomId = roomId,
            UserId = user.UserId,
            BeginTime = beginTime,
            EndTime = endTime,
            Checkout = false
        };

        await dbContext.CheckinRecords.AddAsync(record);
        await dbContext.SaveChangesAsync();

        return record;
    }

    /// <summary>
    /// 查询指定范围内的空调记录
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IEnumerable<AirConditionerRecord> QueryAirConditionerRecords(ObjectId roomId,
        DateTimeOffset begin, DateTimeOffset end)
    {
        IQueryable<AirConditionerRecord> query = from item in dbContext.AirConditionerRecords.AsNoTracking()
            where item.RoomId == roomId
            where item.BeginTime >= begin && item.EndTime <= end
            select item;

        List<AirConditionerRecord> records = [];
        AirConditionerRecord? lastRecord = null;

        foreach (AirConditionerRecord record in query)
        {
            if (lastRecord is null)
            {
                lastRecord = record;
            }
            else
            {
                if (lastRecord.Speed == record.Speed &&
                    lastRecord.EndTime.ToUnixTimeSeconds() == record.BeginTime.ToUnixTimeSeconds())
                {
                    lastRecord.EndTemperature = record.EndTemperature;
                    lastRecord.EndTime = record.EndTime;
                    lastRecord.Fee += record.Fee;
                }
                else
                {
                    records.Add(lastRecord);
                    lastRecord = record;
                }
            }
        }

        if (lastRecord is not null)
        {
            records.Add(lastRecord);
        }

        return records;
    }
}
