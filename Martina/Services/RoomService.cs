using Martina.DataTransferObjects;
using Martina.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

public class RoomService(MartinaDbContext dbContext)
{
    public async Task<IEnumerable<RoomResponse>> ListAllRooms()
    {
        List<RoomResponse> result = [];

        foreach (Room room in dbContext.Rooms.AsNoTracking())
        {
            CheckinRecord? record = await QueryCurrentStatus(room.Id);

            result.Add(record is null ? new RoomResponse(room) : new RoomResponse(room, record));
        }

        return result;
    }

    public async Task<CheckinRecord?> QueryCurrentStatus(ObjectId roomId)
    {
        IQueryable<CheckinRecord> query = from item in dbContext.CheckinRecords.AsNoTracking()
            where item.RoomId == roomId && item.BeginTime <= DateTimeOffset.Now && item.EndTime >= DateTimeOffset.Now
            select item;

        return await query.FirstOrDefaultAsync();
    }
}
