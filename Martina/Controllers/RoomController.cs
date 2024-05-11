using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Controllers;

[ApiController]
[Route("api/room")]
public sealed class RoomController(MartinaDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// 查询所有的房间
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<RoomResponse>>(200)]
    public IActionResult ListAllRooms()
    {
        List<Room> rooms = dbContext.Rooms
            .AsNoTracking()
            .ToList();

        return Ok(rooms.Select(r => new RoomResponse(r)));
    }

    /// <summary>
    /// 列出指定房间的信息
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    [HttpGet("{roomId}")]
    [ProducesResponseType<RoomResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(404)]
    public async Task<IActionResult> GetRoom([FromRoute] string roomId)
    {
        IQueryable<Room> query = from item in dbContext.Rooms.AsNoTracking()
            where item.Id == new ObjectId(roomId)
            select item;

        Room? room = await query.FirstOrDefaultAsync();

        if (room is null)
        {
            return NotFound(new ExceptionMessage("Target room doesn't exist."));
        }
        else
        {
            return Ok(new RoomResponse(room));
        }
    }

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType<RoomResponse>(201)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
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

        return Created($"api/room/{room.Id}", new RoomResponse(room));
    }

    /// <summary>
    /// 修改指定房间的信息
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{roomId}")]
    [ProducesResponseType<RoomResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> UpdateRoomInformation([FromRoute] string roomId,
        [FromBody] CreateRoomRequest request)
    {
        IQueryable<Room> query = from item in dbContext.Rooms
            where item.Id == new ObjectId(roomId)
            select item;

        Room? room = await query.FirstOrDefaultAsync();

        if (room is null)
        {
            return BadRequest(new ExceptionMessage("Target room doesn't exist."));
        }

        room.RoomName = request.RoomName;
        room.Price = request.PricePerDay;
        room.RoomBasicTemperature = request.RoomBasicTemperature;

        await dbContext.SaveChangesAsync();

        return Ok(new RoomResponse(room));
    }

    /// <summary>
    /// 删除指定的房间
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    [HttpDelete("{roomId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ExceptionMessage>(404)]
    public async Task<IActionResult> DeleteRoom([FromRoute] string roomId)
    {
        IQueryable<Room> query = from item in dbContext.Rooms
            where item.Id == new ObjectId(roomId)
            select item;

        Room? room = await query.FirstOrDefaultAsync();

        if (room is null)
        {
            return NotFound(new ExceptionMessage("Target room doesn't exist."));
        }

        dbContext.Rooms.Remove(room);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
