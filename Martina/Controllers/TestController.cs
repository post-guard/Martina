using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Models;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Martina.Controllers;

[ApiController]
[Route("api/test")]
[Authorize(policy: "Administrator")]
public class TestController(
    AirConditionerManageService airConditionerManageService,
    AirConditionerTestService airConditionerTestService) : ControllerBase
{
    private static CancellationTokenSource s_stopptingTokenSource = new();

    private static Task? s_runningTask;

    /// <summary>
    /// 发起测试请求
    /// </summary>
    /// <remarks>
    /// caseName: hot | cool
    /// </remarks>
    /// <param name="caseName"></param>
    /// <returns></returns>
    [HttpPatch("start")]
    [ProducesResponseType<IEnumerable<CheckinResponse>>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> StartTest([FromQuery] string caseName)
    {
        if (s_runningTask is {IsCompleted: false})
        {
            return BadRequest(new ExceptionMessage("存在正在运行的测试"));
        }

        if (s_runningTask is { IsCompleted: true })
        {
            await s_runningTask;
            s_runningTask = null;
        }

        IEnumerable<CheckinRecord> records;
        s_stopptingTokenSource = new CancellationTokenSource();

        if (caseName == "hot")
        {
            airConditionerManageService.Opening = true;
            airConditionerManageService.Option = new AirConditionerOption
            {
                Cooling = false, TemperatureThreshold = decimal.One / 2, BackSpeed = decimal.One / 2
            };

            Dictionary<string, Room> rooms =
                await airConditionerTestService.CreateTestRoom(AirConditionerTestCases.HotRooms);
            records = await airConditionerTestService.CreateCheckinRecords(rooms,
                AirConditionerTestCases.HotCheckinRecords);

            s_runningTask = airConditionerTestService.SendAirConditionerRequests(rooms,
                AirConditionerTestCases.HotCases, s_stopptingTokenSource.Token);
        }
        else if (caseName == "cool")
        {
            airConditionerManageService.Opening = true;
            airConditionerManageService.Option = new AirConditionerOption
            {
                Cooling = true, TemperatureThreshold = decimal.One / 2, BackSpeed = decimal.One / 2
            };

            Dictionary<string, Room> rooms =
                await airConditionerTestService.CreateTestRoom(AirConditionerTestCases.CoolRooms);
            records = await airConditionerTestService.CreateCheckinRecords(rooms,
                AirConditionerTestCases.CoolCheckinRecords);

            s_runningTask = airConditionerTestService.SendAirConditionerRequests(rooms,
                AirConditionerTestCases.CoolCases, s_stopptingTokenSource.Token);
        }
        else
        {
            return BadRequest(new ExceptionMessage("指定的测试集不存在"));
        }

        return Ok(records.Select(r => new CheckinResponse(r)));
    }

    /// <summary>
    /// 停止正在进行的测试
    /// </summary>
    /// <returns></returns>
    [HttpPatch("stop")]
    [ProducesResponseType(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> StopTest()
    {
        if (s_runningTask is { IsCompleted: false })
        {
            await s_stopptingTokenSource.CancelAsync();
            await s_runningTask;
            s_runningTask = null;

            return Ok();
        }

        if (s_runningTask is {IsCompleted: true})
        {
            await s_runningTask;
            s_runningTask = null;

            return Ok();
        }

        return BadRequest(new ExceptionMessage("没有正在运行的任务"));
    }

    /// <summary>
    /// 清除指定测试集造成的影响
    /// </summary>
    /// <remarks>
    /// casename : hot | cool
    /// </remarks>
    /// <param name="caseName"></param>
    /// <returns></returns>
    [HttpPatch("clear")]
    [ProducesResponseType(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> ClearTest([FromQuery] string caseName)
    {
        if (caseName == "hot")
        {
            await airConditionerTestService.ClearTestRecord(AirConditionerTestCases.HotRooms);
            return Ok();
        }

        if (caseName == "cool")
        {
            await airConditionerTestService.ClearTestRecord(AirConditionerTestCases.CoolRooms);
            return Ok();
        }

        return BadRequest(new ExceptionMessage("指定的测试集不存在"));
    }
}
