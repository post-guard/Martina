using Martina.DataTransferObjects;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Martina.Controllers;

[ApiController]
[Route("api/room")]
public sealed class RoomController(RoomService roomService) : ControllerBase
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
        RoomResponse? response = await roomService.FindRoomById(roomId);

        if (response is null)
        {
            return NotFound(new ExceptionMessage("查询的房间不存在！"));
        }

        return Ok(response);
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
        RoomResponse response = await roomService.CreateRoom(request);

        return Created($"api/room/{response.RoomId}", response);
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
        bool result = await roomService.DeleteRoom(roomId);

        if (result)
        {
            return NoContent();
        }

        return NotFound("指定的房间不存在！");
    }
}
