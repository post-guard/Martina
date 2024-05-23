using System.Net.WebSockets;
using System.Text.Json;
using Martina.Abstractions;
using Martina.DataTransferObjects;
using Martina.Entities;
using Martina.Models;
using Martina.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Martina.Controllers;

[ApiController]
[Route("api/airConditioner")]
public class AirConditionerController(
    IAuthorizationService authorizationService,
    ISchedular schedular,
    AirConditionerManageService airConditionerManageService,
    MartinaDbContext dbContext,
    CheckinService checkinService,
    ILogger<AirConditionerController> logger) : ControllerBase
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

        return Ok(checkinService.QueryAirConditionerRecords(objectId, DateTimeOffset.FromUnixTimeSeconds(beginTime),
                DateTimeOffset.FromUnixTimeSeconds(endTime))
            .Select(r => new AirConditionerRecordResponse(r)));
    }


    /// <summary>
    /// 发起空调服务请求
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="request"></param>
    /// <remarks>
    /// 要求发起请求的用户是当前房间的住户
    /// </remarks>
    /// <returns></returns>
    [HttpPost("{roomId}")]
    [Authorize]
    public async Task<IActionResult> RequestAirConditionor([FromRoute] string roomId,
        [FromBody] AirConditionerRequest request)
    {
        if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
        {
            return BadRequest(new ExceptionMessage("非法的房间ID！"));
        }

        Room? room = await dbContext.Rooms.AsNoTracking()
            .Where(r => r.Id == roomObjectId)
            .FirstOrDefaultAsync();

        if (room is null)
        {
            return NotFound(new ExceptionMessage("房间不存在！"));
        }

        AuthorizationResult result = await authorizationService.AuthorizeAsync(User, room, [new CheckinRequirement()]);

        if (!result.Succeeded)
        {
            return Forbid();
        }

        if (!airConditionerManageService.VolidateAirConditionerRequest(roomObjectId, request, out string? message))
        {
            return BadRequest(new ExceptionMessage(message));
        }

        logger.LogInformation("Receive from {}: {}.", room.RoomName, request);
        await schedular.SendRequest(room.Id, request);
        return Ok();
    }

    [Route("ws")]
    public async Task PushRoomsInformation()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        CancellationTokenSource stoppingTokenSource = new();
        Task sendTask = SendRoomsInformation(webSocket, stoppingTokenSource.Token);

        byte[] buffer = new byte[1024 * 4];

        while (true)
        {
            WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            if (receiveResult.CloseStatus.HasValue)
            {
                await stoppingTokenSource.CancelAsync();
                await sendTask;

                await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription,
                    CancellationToken.None);
                break;
            }
        }
    }

    private async Task SendRoomsInformation(WebSocket webSocket, CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

        try
        {
            await webSocket.SendAsync(GenerateMessage(), WebSocketMessageType.Text, true, stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await webSocket.SendAsync(GenerateMessage(), WebSocketMessageType.Text, true, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Route("ws/{roomId}")]
    public async Task PushRoomInformation([FromRoute] string roomId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        Room? room = await dbContext.Rooms.AsNoTracking()
            .Where(r => r.Id == roomObjectId)
            .FirstOrDefaultAsync();

        if (room is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        CancellationTokenSource stoppingTokenSource = new();

        Task sendTask = SendRoomInformation(room.Id, webSocket, stoppingTokenSource.Token);
        byte[] buffer = new byte[1024 * 4];

        while (true)
        {
            WebSocketReceiveResult receiveResult =
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receiveResult.CloseStatus.HasValue)
            {
                await stoppingTokenSource.CancelAsync();
                await sendTask;

                await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription,
                    CancellationToken.None);
                break;
            }
        }
    }

    private async Task SendRoomInformation(ObjectId roomId, WebSocket webSocket, CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

        try
        {
            await webSocket.SendAsync(GenerateMessage(roomId), WebSocketMessageType.Text, true, stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await webSocket.SendAsync(GenerateMessage(roomId), WebSocketMessageType.Text, true, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private Memory<byte> GenerateMessage()
    {
        List<AirConditionerResponse> response = schedular.States.Values
            .Select(s => new AirConditionerResponse(s))
            .ToList();

        return JsonSerializer.SerializeToUtf8Bytes(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private Memory<byte> GenerateMessage(ObjectId roomId)
    {
        AirConditionerResponse response = new(schedular.States[roomId]);

        return JsonSerializer.SerializeToUtf8Bytes(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
