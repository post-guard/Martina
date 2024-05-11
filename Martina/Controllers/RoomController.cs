using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Controllers;

[ApiController]
[Route("api/room")]
public sealed class RoomController(MartinaDbContext dbContext, RoomService roomService) : ControllerBase
{
    /// <summary>
    /// 查询所有的房间
    /// </summary>
    /// <remarks>
    /// 登录即可获取
    /// </remarks>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<RoomResponse>>(200)]
    [Authorize]
    public async Task<IActionResult> ListAllRooms()
    {
        return Ok(await roomService.ListAllRooms());
    }

    /// <summary>
    /// 列出指定房间的信息
    /// </summary>
    /// <remarks>
    /// 登录即可访问
    /// </remarks>
    /// <param name="roomId"></param>
    /// <returns></returns>
    [HttpGet("{roomId}")]
    [ProducesResponseType<RoomResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(404)]
    [Authorize]
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

        CheckinRecord? record = await roomService.QueryCurrentStatus(room.Id);

        return Ok(record is null ? new RoomResponse(room) : new RoomResponse(room, record));
    }

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <remarks>
    /// 需要超级管理员权限
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType<RoomResponse>(201)]
    [Authorize(policy: "Administrator")]
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
    /// <remarks>
    /// 需要超级管理员权限
    /// </remarks>
    /// <param name="roomId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{roomId}")]
    [ProducesResponseType<RoomResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    [Authorize(policy: "Administrator")]
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
    /// <remarks>
    /// 需要超级管理员权限
    /// </remarks>
    /// <param name="roomId"></param>
    /// <returns></returns>
    [HttpDelete("{roomId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ExceptionMessage>(404)]
    [Authorize(policy: "Administrator")]
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
