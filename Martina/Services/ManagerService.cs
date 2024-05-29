using Martina.Entities;
using Martina.Models;
using Microsoft.EntityFrameworkCore;

namespace Martina.Services;

public class ManagerService(MartinaDbContext dbContext)
{
    public Task<int> QueryCurrentUser()
    {
        List<CheckinRecord> records = (from item in dbContext.CheckinRecords.AsNoTracking()
            where item.BeginTime <= TimeService.Now && item.EndTime <= TimeService.Now
            select item).ToList();

        return Task.FromResult(records.Select(r => r.UserId).Distinct().Count());
    }

    public Task<int> QueryCurrentCheckin()
    {
        IQueryable<CheckinRecord> records = from item in dbContext.CheckinRecords.AsNoTracking()
            where item.BeginTime <= TimeService.Now && item.EndTime <= TimeService.Now
            select item;

        return Task.FromResult(records.Count());
    }

    public async Task<List<DailyRevenue>> QueryDailyRevenue(DateTimeOffset begin, DateTimeOffset end)
    {
        List<DailyRevenue> result = [];

        while (begin < end)
        {
            DateTimeOffset start = begin;
            DateTimeOffset stop = begin.AddDays(1);

            List<CheckinRecord> checkinRecords = (from item in dbContext.CheckinRecords.AsNoTracking()
                where item.BeginTime <= stop || item.EndTime >= start
                select item).ToList();

            List<decimal> checkinFees = [];

            foreach (CheckinRecord record in checkinRecords)
            {
                Room? room = await (from item in dbContext.Rooms.AsNoTracking()
                    where item.Id == record.RoomId
                    select item).FirstOrDefaultAsync();

                if (room is null)
                {
                    continue;
                }

                checkinFees.Add(room.Price);
            }

            List<AirConditionerRecord> airConditionerRecords =
                (from item in dbContext.AirConditionerRecords.AsNoTracking()
                    where item.BeginTime <= stop || item.EndTime >= start
                    select item).ToList();

            result.Add(new DailyRevenue
            {
                Day = new DateTimeOffset(new DateTime(start.Year, start.Month, start.Day)),
                RoomRevenue = checkinFees.Sum(),
                AirConditionerRevenue = airConditionerRecords.Select(r => r.Fee).Sum()
            });

            begin = begin.AddDays(1);
        }

        return result;
    }
}
