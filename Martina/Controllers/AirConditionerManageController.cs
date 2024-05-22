using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Models;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Martina.Controllers;

[ApiController]
[Route("api/airConditionerManage")]
[Authorize(policy: "AirConditionerAdministrator")]
public class AirConditionerManageController(
    AirConditionerManageService airConditionerManageService,
    ISchedular schedular) : ControllerBase
{
    /// <summary>
    /// 获得空调系统的配置信息
    /// </summary>
    /// <remarks>
    /// 当空调系统尚未开启时返回400
    /// </remarks>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<AirConditionerOption>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    [Authorize]
    public IActionResult GetConfiguration()
    {
        if (!airConditionerManageService.Opening)
        {
            return BadRequest(new ExceptionMessage("尚未开启空调系统"));
        }

        return Ok(airConditionerManageService.Option);
    }

    /// <summary>
    /// 修改空调系统的配置信息
    /// </summary>
    /// <remarks>
    /// 当空调系统尚未开启时返回400
    /// </remarks>
    /// <param name="option"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType<AirConditionerOption>(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> ConfigureAirCondinioner([FromBody] AirConditionerOption option)
    {
        if (!airConditionerManageService.Opening)
        {
            return BadRequest(new ExceptionMessage("尚未开启空调系统"));
        }

        airConditionerManageService.Option = option;
        await schedular.Reset();

        return Ok(airConditionerManageService.Option);
    }

    /// <summary>
    /// 开启空调系统
    /// </summary>
    /// <returns></returns>
    [HttpPut("open")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> OpenAirConditioner()
    {
        airConditionerManageService.Opening = true;
        await schedular.Reset();

        return Ok();
    }

    /// <summary>
    /// 关闭空调系统
    /// </summary>
    /// <returns></returns>
    [HttpPut("close")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CloseAirConditioner()
    {
        airConditionerManageService.Opening = false;
        await schedular.Reset();

        return Ok();
    }

    /// <summary>
    /// 重置空调系统为初始状态
    /// </summary>
    /// <remarks>
    /// 空调系统尚未开启时返回400
    /// </remarks>
    /// <returns></returns>
    [HttpPost("reset")]
    [ProducesResponseType(200)]
    [ProducesResponseType<ExceptionMessage>(400)]
    public async Task<IActionResult> ResetAirConditioner()
    {
        if (!airConditionerManageService.Opening)
        {
            return BadRequest("尚未开启空调系统");
        }

        await schedular.Reset();
        return Ok();
    }
}
