using Martina.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

public class BillService(MartinaDbContext dbContext)
{
    public IEnumerable<AirConditionerRecord> QueryAirConditionerRecords(ObjectId roomId,
        DateTimeOffset begin, DateTimeOffset end)
    {
        IQueryable<AirConditionerRecord> query = from item in dbContext.AirConditionerRecords.AsNoTracking()
            where item.RoomId == roomId
            where item.BeginTime >= begin && item.EndTime <= end
            select item;

        return query.ToList();
    }
}
