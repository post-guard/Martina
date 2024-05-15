using System.Net.WebSockets;
using System.Text.Json;
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
[Route("api/airConditionor")]
public class AirConditionorController(
    IAuthorizationService authorizationService,
    MartinaDbContext dbContext,
    ILogger<AirConditionorController> logger) : ControllerBase
{
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
    public async Task<IActionResult> RequestAirConditionor([FromRoute] string roomId,
        [FromBody] AirConditionorRequest request)
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

        return Ok();
    }

    [Route("ws")]
    [Authorize(policy: "AirConditionorAdministrator")]
    public async Task PushRoomsInformation()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        logger.LogInformation("Receive websocket connection");

        List<AirConditionorResponse> responses = [];

        foreach (Room room in dbContext.Rooms.AsNoTracking())
        {
            responses.Add(new AirConditionorResponse
            {
                RoomId = room.Id.ToString(),
                Opening = false
            });
        }

        Memory<byte> sendMessage =
            JsonSerializer.SerializeToUtf8Bytes(responses, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        CancellationTokenSource stoppingTokenSource = new();

        Task sendTask = SendAirConditionorInformation(sendMessage, webSocket, stoppingTokenSource.Token);

        byte[] buffer = new byte[1024 * 4];

        while (true)
        {
            WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            if (receiveResult.CloseStatus.HasValue)
            {
                try
                {
                    await stoppingTokenSource.CancelAsync();
                    await sendTask;
                }
                catch (OperationCanceledException)
                {
                }

                logger.LogInformation("Close connection.");
                await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription,
                    CancellationToken.None);
                break;
            }
        }
    }

    [Route("ws/{roomId}")]
    public async Task PushRoomInformation([FromRoute] string roomId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        if (!ObjectId.TryParse(roomId, out ObjectId roomObjectId))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        Room? room = await dbContext.Rooms.AsNoTracking()
            .Where(r => r.Id == roomObjectId)
            .FirstOrDefaultAsync();

        if (room is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        AuthorizationResult result = await authorizationService.AuthorizeAsync(User, room, [new CheckinRequirement()]);

        if (!result.Succeeded)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        logger.LogInformation("Receive websocket connection.");

        CancellationTokenSource stoppingTokenSource = new();

        Memory<byte> sendMessage = JsonSerializer.SerializeToUtf8Bytes(
            new AirConditionorResponse { RoomId = room.Id.ToString(), Opening = false },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Task sendTask = SendAirConditionorInformation(sendMessage, webSocket, stoppingTokenSource.Token);
        byte[] buffer = new byte[1024 * 4];

        while (true)
        {
            WebSocketReceiveResult receiveResult =
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receiveResult.CloseStatus.HasValue)
            {
                await stoppingTokenSource.CancelAsync();
                try
                {
                    await sendTask;
                }
                catch (OperationCanceledException)
                {
                }

                logger.LogInformation("Close connection.");
                await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription,
                    CancellationToken.None);
                break;
            }
        }
    }

    private async Task SendAirConditionorInformation(Memory<byte> content, WebSocket webSocket, CancellationToken token)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        logger.LogInformation("Send message.");
        await webSocket.SendAsync(content, WebSocketMessageType.Text, true, token);

        while (await timer.WaitForNextTickAsync(token))
        {
            logger.LogInformation("Send message.");
            await webSocket.SendAsync(content, WebSocketMessageType.Text, true, token);
        }
    }
}
