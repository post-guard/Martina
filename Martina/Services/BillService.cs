using Martina.Controllers;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Exceptions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

public class BillService(MartinaDbContext dbContext, CheckinService checkinService)
{
    /// <summary>
    /// 查询当前数据库中的账单
    /// </summary>
    /// <param name="userId">查询的用户ID</param>
    /// <param name="roomId">查询的房间ID</param>
    /// <param name="begin">查询的开始时间</param>
    /// <param name="end">查询的结束时间</param>
    /// <returns></returns>
    public List<BillRecord> QueryBillRecord(string? userId, string? roomId, DateTimeOffset begin, DateTimeOffset end)
    {
        IQueryable<BillRecord> records = dbContext.BillRecords.AsNoTracking();

        if (userId is not null)
        {
            records = from item in records
                where item.UserId == userId
                select item;
        }

        if (roomId is not null)
        {
            if (ObjectId.TryParse(roomId, out ObjectId id))
            {
                records = from item in records
                    where item.Id == id
                    select item;
            }
        }

        records = from item in records
            where item.BeginTime >= begin && item.EndTime <= end
            select item;

        return records.ToList();
    }

    /// <summary>
    /// 办理结账
    /// </summary>
    /// <param name="checkinIds">需要结账的入住记录列表</param>
    /// <returns></returns>
    public async Task<BillResponse> Checkout(IEnumerable<ObjectId> checkinIds)
    {
        BillRecord record = await GenerateBillRecord(checkinIds);

        record.Id = ObjectId.GenerateNewId();

        await dbContext.BillRecords.AddAsync(record);
        await dbContext.SaveChangesAsync();

        List<CheckinResponse> checkinResponse = [];

        foreach (ObjectId id in record.CheckinRecords)
        {
            CheckinRecord checkinRecord = await dbContext.CheckinRecords
                .Where(r => r.Id == id)
                .FirstAsync();

            checkinRecord.Checkout = true;
            checkinResponse.Add(new CheckinResponse(checkinRecord));
        }

        await dbContext.SaveChangesAsync();

        List<AirConditionerRecordResponse> airConditionerRecordResponse = [];

        foreach (ObjectId id in record.AirConditionerRecords)
        {
            AirConditionerRecord airConditionerRecord = await dbContext.AirConditionerRecords
                .Where(r => r.Id == id)
                .FirstAsync();

            airConditionerRecord.Checked = true;
            airConditionerRecordResponse.Add(new AirConditionerRecordResponse(airConditionerRecord));
        }

        await dbContext.SaveChangesAsync();

        return new BillResponse
        {
            Id = record.Id.ToString(),
            UserId = record.UserId,
            BeginTime = record.BeginTime.ToUnixTimeSeconds(),
            EndTime = record.EndTime.ToUnixTimeSeconds(),
            CheckinResponses = checkinResponse,
            AirConditionerRecordResponses = airConditionerRecordResponse,
            RoomFee = record.RoomFee,
            AirConditionerFee = record.AirConditionerFee
        };
    }

    /// <summary>
    /// 生成账单预览
    /// </summary>
    /// <param name="checkinIds">欲结账的入住记录列表</param>
    /// <returns></returns>
    /// <exception cref="BillException"></exception>
    public async Task<BillRecord> GenerateBillRecord(IEnumerable<ObjectId> checkinIds)
    {
        List<CheckinRecord> records = [];

        foreach (ObjectId id in checkinIds)
        {
            IQueryable<CheckinRecord> query = from item in dbContext.CheckinRecords.AsNoTracking()
                where item.Id == id
                select item;

            CheckinRecord? record = await query.FirstOrDefaultAsync();

            if (record is null)
            {
                throw new BillException("指定的入住记录不存在");
            }

            records.Add(record);
        }

        if ((from record in records select record.UserId).Any(i => i != records[0].UserId))
        {
            throw new BillException("指定的入住记录不属于同一位顾客");
        }

        List<AirConditionerRecord> airConditionerRecords = records
            .Select(r => checkinService.QueryAirConditionerRecords(r.RoomId, r.BeginTime, r.EndTime))
            .Aggregate(new List<AirConditionerRecord>(), (result, middle) =>
            {
                result.AddRange(middle);
                return result;
            });

        decimal roomFee = 0;

        foreach (CheckinRecord record in records)
        {
            Room room = await dbContext.Rooms.AsNoTracking()
                .Where(r => r.Id == record.RoomId)
                .FirstAsync();

            TimeSpan liveDay = record.EndTime - record.BeginTime;
            roomFee += room.Price * liveDay.Days;
        }

        BillRecord response = new()
        {
            UserId = records[0].UserId,
            BeginTime = records.Select(r => r.BeginTime).Min(),
            EndTime = records.Select(r => r.EndTime).Max(),
            CheckinRecords = records.Select(r => r.Id).ToList(),
            AirConditionerRecords = airConditionerRecords.Select(r => r.Id).ToList(),
            RoomFee = roomFee,
            AirConditionerFee = airConditionerRecords.Select(r => r.Fee).Sum()
        };

        return response;
    }

    public async Task<BillResponse> GenerateBillResponse(BillRecord billRecord)
    {
        List<CheckinResponse> checkinResponse = [];

        foreach (ObjectId id in billRecord.CheckinRecords)
        {
            checkinResponse.Add(new CheckinResponse(await dbContext.CheckinRecords.AsNoTracking()
                .Where(r => r.Id == id)
                .FirstAsync()));
        }

        List<AirConditionerRecordResponse> airConditionerRecordResponse = [];

        foreach (ObjectId id in billRecord.AirConditionerRecords)
        {
            airConditionerRecordResponse.Add(new AirConditionerRecordResponse(await dbContext.AirConditionerRecords
                .AsNoTracking()
                .Where(r => r.Id == id)
                .FirstAsync()));
        }

        return new BillResponse
        {
            Id = billRecord.Id == ObjectId.Empty ? null : billRecord.Id.ToString(),
            UserId = billRecord.UserId,
            BeginTime = billRecord.BeginTime.ToUnixTimeSeconds(),
            EndTime = billRecord.EndTime.ToUnixTimeSeconds(),
            CheckinResponses = checkinResponse,
            AirConditionerRecordResponses = airConditionerRecordResponse,
            RoomFee = billRecord.RoomFee,
            AirConditionerFee = billRecord.AirConditionerFee
        };
    }
}
