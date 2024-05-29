using Martina.DataTransferObjects;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Martina.Controllers;

[Route("api/manager")]
[ApiController]
public class ManagerController(ManagerService managerService) : ControllerBase
{
    /// <summary>
    /// 查询酒店的收入趋势
    /// </summary>
    /// <remarks>
    /// begin 合法的日期字符串 建议格式为 yyyy-MM-dd
    ///
    /// end 合法的日期字符串 建议格式为 yyyy-MM-dd
    /// </remarks>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    [HttpGet("revenue")]
    [Authorize(policy: "BillAdministrator")]
    [ProducesResponseType<ExceptionMessage>(400)]
    [ProducesResponseType<RevenueTrend>(200)]
    public async Task<IActionResult> QueryRevenueTrend([FromQuery] DateTimeOffset begin, [FromQuery] DateTimeOffset end)
    {
        if (begin >= end)
        {
            return BadRequest(new ExceptionMessage("开始时间不能晚于结束时间"));
        }

        RevenueTrend trend = new()
        {
            TotalUsers = await managerService.QueryCurrentUser(),
            TotalCheckin = await managerService.QueryCurrentCheckin(),
            DailyRevenues = await managerService.QueryDailyRevenue(begin, end)
        };

        return Ok(trend);
    }
}
