using Martina.DataTransferObjects;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Controllers;

[ApiController]
[Route("api/bill")]
public class BillController(BillService billService, MartinaDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// 查询空调详单
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="beginTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    [HttpGet("airConditionerRecords")]
    [ProducesResponseType<IEnumerable<AirConditionerRecordResponse>>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    [Authorize]
    public async Task<IActionResult> QueryAirConditionerRecords([FromQuery] string roomId,
        [FromQuery] long beginTime = 0,
        [FromQuery] long endTime = 253402300790)
    {
        if (!ObjectId.TryParse(roomId, out ObjectId objectId))
        {
            return BadRequest(new ExceptionMessage("指定的房间不存在"));
        }

        if (!await dbContext.Rooms.AsNoTracking().AnyAsync(r => r.Id == objectId))
        {
            return BadRequest(new ExceptionMessage("指定的房间不存在"));
        }

        return Ok(billService.QueryAirConditionerRecords(objectId, DateTimeOffset.FromUnixTimeSeconds(beginTime),
                DateTimeOffset.FromUnixTimeSeconds(endTime))
            .Select(r => new AirConditionerRecordResponse(r)));
    }
}
