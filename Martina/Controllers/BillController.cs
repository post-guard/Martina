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
[Route("api/bill")]
public class BillController(BillService billService, MartinaDbContext dbContext)
    : ControllerBase
{
    /// <summary>
    /// 查询符合条件的账单
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roomId"></param>
    /// <param name="beginTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [ProducesResponseType<IEnumerable<BillResponse>>(200)]
    public async Task<IActionResult> QueryBills([FromQuery] string? userId, [FromQuery] string? roomId,
        [FromQuery] long beginTime = 0, [FromQuery] long endTime = 253402300790)
    {
        List<BillRecord> records = billService.QueryBillRecord(userId, roomId,
            DateTimeOffset.FromUnixTimeSeconds(beginTime), DateTimeOffset.FromUnixTimeSeconds(endTime));

        List<BillResponse> responses = [];

        foreach (BillRecord record in records)
        {
            responses.Add(await billService.GenerateBillResponse(record));
        }

        return Ok(responses);
    }


    /// <summary>
    /// 获取指定的账单ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType<BillResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBill([FromRoute] string id)
    {
        if (!ObjectId.TryParse(id, out ObjectId billId))
        {
            return BadRequest(new ExceptionMessage("无效的对象ID"));
        }

        IQueryable<BillRecord> recordQuery = from item in dbContext.BillRecords.AsNoTracking()
            where item.Id == billId
            select item;

        BillRecord? record = await recordQuery.FirstOrDefaultAsync();

        if (record is null)
        {
            return NotFound();
        }

        return Ok(await billService.GenerateBillResponse(record));
    }

    /// <summary>
    /// 获得预览的账单
    /// </summary>
    /// <param name="checkinRecordIds"></param>
    /// <returns></returns>
    [HttpPost("preview")]
    [Authorize(policy: "RoomAdministrator")]
    [ProducesResponseType<BillResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> GenerateBillPreview([FromBody] string[] checkinRecordIds)
    {
        List<ObjectId> checkinRecords = [];

        foreach (string id in checkinRecordIds)
        {
            if (!ObjectId.TryParse(id, out ObjectId recordId))
            {
                return BadRequest(new ExceptionMessage("非法的对象ID"));
            }

            checkinRecords.Add(recordId);
        }

        BillRecord record;
        try
        {
            record = await billService.GenerateBillRecord(checkinRecords);
        }
        catch (BillException e)
        {
            return BadRequest(new ExceptionMessage(e));
        }

        return Ok(await billService.GenerateBillResponse(record));
    }

    /// <summary>
    /// 结账
    /// </summary>
    /// <param name="checkinRecordIds"></param>
    /// <returns></returns>
    [HttpPost("checkout")]
    [Authorize(policy: "RoomAdministrator")]
    [ProducesResponseType<BillResponse>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> Checkout([FromBody] string[] checkinRecordIds)
    {
        List<ObjectId> checkinRecords = [];

        foreach (string id in checkinRecordIds)
        {
            if (!ObjectId.TryParse(id, out ObjectId recordId))
            {
                return BadRequest(new ExceptionMessage("非法的对象ID"));
            }

            checkinRecords.Add(recordId);
        }

        BillResponse response;
        try
        {
            response = await billService.Checkout(checkinRecords);
        }
        catch (BillException e)
        {
            return BadRequest(new ExceptionMessage(e));
        }

        return Ok(response);
    }
}
