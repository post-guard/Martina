using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Services;

public class RoomService(MartinaDbContext dbContext, ISchedular schedular)
{
    public async Task<IEnumerable<RoomResponse>> ListAllRooms()
    {
        List<RoomResponse> result = [];

        foreach (Room room in dbContext.Rooms.AsNoTracking())
        {
            CheckinRecord? record = await QueryRoomCurrentStatus(room.Id);
            AirConditionerState state = schedular.States[room.Id];

            result.Add(record is null ? new RoomResponse(room, state) : new RoomResponse(room, record, state));
        }

        return result;
    }

    public async Task<RoomResponse?> FindRoomById(string id)
    {
        if (!ObjectId.TryParse(id, out ObjectId roomId))
        {
            return null;
        }

        IQueryable<Room> query = from item in dbContext.Rooms.AsNoTracking()
            where item.Id == roomId
            select item;

        Room? room = await query.FirstOrDefaultAsync();

        if (room is null)
        {
            return null;
        }

        CheckinRecord? record = await QueryRoomCurrentStatus(room.Id);
        AirConditionerState state = schedular.States[room.Id];

        return record is null ? new RoomResponse(room, state) : new RoomResponse(room, record, state);
    }

    public async Task<CheckinRecord?> QueryRoomCurrentStatus(ObjectId roomId)
    {
        IQueryable<CheckinRecord> query = from item in dbContext.CheckinRecords.AsNoTracking()
            where item.RoomId == roomId && item.BeginTime <= DateTimeOffset.Now && item.EndTime >= DateTimeOffset.Now
            select item;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<CheckinRecord?> QueryUserCurrentStatus(string userId)
    {
        IQueryable<CheckinRecord> query = from item in dbContext.CheckinRecords.AsNoTracking()
            where item.UserId == userId && item.BeginTime <= DateTimeOffset.Now && item.EndTime >= DateTimeOffset.Now
            select item;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<RoomResponse> CreateRoom(CreateRoomRequest request)
    {
        Room room = new()
        {
            Id = ObjectId.GenerateNewId(),
            RoomName = request.RoomName,
            Price = request.PricePerDay,
            RoomBasicTemperature = request.RoomBasicTemperature
        };

        await dbContext.Rooms.AddAsync(room);
        await dbContext.SaveChangesAsync();

        await schedular.Reset();

        return new RoomResponse(room, schedular.States[room.Id]);
    }

    public async Task<bool> DeleteRoom(string id)
    {
        if (!ObjectId.TryParse(id, out ObjectId roomId))
        {
            return false;
        }

        IQueryable<Room> query = from item in dbContext.Rooms
            where item.Id == roomId
            select item;

        Room? room = await query.FirstOrDefaultAsync();

        if (room is null)
        {
            return false;
        }

        dbContext.Rooms.Remove(room);
        await dbContext.SaveChangesAsync();

        await schedular.Reset();

        return true;
    }
}
