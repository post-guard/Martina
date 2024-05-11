using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Exceptions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

public class CheckinService(MartinaDbContext dbContext, UserService userService)
{
    public List<CheckinRecord> QueryCheckinRecords(string? roomId, string? userId, long begin, long end)
    {
        DateTimeOffset beginTime = DateTimeOffset.FromUnixTimeSeconds(begin);
        DateTimeOffset endTime = DateTimeOffset.FromUnixTimeSeconds(end);

        IQueryable<CheckinRecord> records = from item in dbContext.CheckinRecords.AsNoTracking()
            where item.BeginTime >= beginTime && item.EndTime <= endTime
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

    public async Task<CheckinRecord> Checkin(CheckinRequest request)
    {
        ObjectId roomId = new(request.RoomId);
        DateTimeOffset beginTime = DateTimeOffset.FromUnixTimeSeconds(request.BeginTime);
        DateTimeOffset endTime = DateTimeOffset.FromUnixTimeSeconds(request.EndTime);

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
            where item.RoomId == roomId && (item.BeginTime < endTime || item.EndTime > beginTime)
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
}
