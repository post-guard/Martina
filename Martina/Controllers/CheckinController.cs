using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Exceptions;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Controllers;

[ApiController]
[Route("api/checkin")]
[Authorize(policy: "RoomAdministrator")]
public sealed class CheckinController(MartinaDbContext dbContext, CheckinService checkinService) : ControllerBase
{
    /// <summary>
    /// 查询所有的入住记录
    /// </summary>
    /// <remarks>
    /// 需要房间管理员及以上的权限
    /// </remarks>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<CheckinResponse>>(200)]
    public IActionResult QueryCheckinRecords([FromQuery] string? roomId = null, [FromQuery] string? userId = null,
        [FromQuery] long beginTime = 0, [FromQuery] long endTime = 253402300790)
    {
        List<CheckinRecord> records = checkinService.QueryCheckinRecords(roomId, userId, beginTime, endTime);

        return Ok(records.Select(r => new CheckinResponse(r)));
    }

    /// <summary>
    /// 列出指定的入住记录
    /// </summary>
    /// <remarks>
    /// 需要房间管理员及以上的权限
    /// </remarks>
    /// <param name="recordId"></param>
    /// <returns></returns>
    [HttpGet("{recordId}")]
    [ProducesResponseType<CheckinResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(404)]
    public async Task<IActionResult> GetCheckinRecord([FromRoute] string recordId)
    {
        IQueryable<CheckinRecord> records = from item in dbContext.CheckinRecords.AsNoTracking()
            where item.Id == new ObjectId(recordId)
            select item;

        CheckinRecord? record = await records.FirstOrDefaultAsync();

        if (record is null)
        {
            return NotFound(new ExceptionMessage("Target record doesn't exist."));
        }
        else
        {
            return Ok(new CheckinResponse(record));
        }
    }

    /// <summary>
    /// 创建指定的入住记录
    /// </summary>
    /// <remarks>
    /// 需要房间管理员及以上的权限
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType<CheckinResponse>(201)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> Checkin([FromBody] CheckinRequest request)
    {
        try
        {
            CheckinRecord record = await checkinService.Checkin(request);

            return Created($"api/checkin/{record.Id}", new CheckinResponse(record));
        }
        catch (CheckinException e)
        {
            return BadRequest(new ExceptionMessage(e));
        }
    }

    /// <summary>
    /// 删除指定的入住记录
    /// </summary>
    /// <remarks>
    /// 需要房间管理员及以上的权限
    /// </remarks>
    /// <param name="recordId"></param>
    /// <returns></returns>
    [HttpDelete("{recordId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ExceptionMessage>(404)]
    public async Task<IActionResult> DeleteCheckin([FromRoute] string recordId)
    {
        IQueryable<CheckinRecord> records = from item in dbContext.CheckinRecords
            where item.Id == new ObjectId(recordId)
            select item;

        CheckinRecord? record = await records.FirstOrDefaultAsync();

        if (record is null)
        {
            return NotFound(new ExceptionMessage("Target record doesn't exist."));
        }
        else
        {
            dbContext.CheckinRecords.Remove(record);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
