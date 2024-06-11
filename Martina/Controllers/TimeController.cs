using System.Net.WebSockets;
using System.Text.Json;
using Martina.DataTransferObjects;
using Martina.Services;
using Microsoft.AspNetCore.Mvc;

namespace Martina.Controllers;

[ApiController]
[Route("api/time")]
public class TimeController : ControllerBase
{
    /// <summary>
    /// 获得当前的系统时间
    /// </summary>
    /// <returns></returns>
    [HttpGet("now")]
    [ProducesResponseType<TimeResponse>(200)]
    public IActionResult GetTime()
    {
        return Ok(new TimeResponse
        {
            Now = TimeService.Now.ToUnixTimeSeconds()
        });
    }


    [Route("")]
    public async Task PushTime()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        CancellationTokenSource stoppingTokenSource = new();
        Task sendTask = SendTime(webSocket, stoppingTokenSource.Token);
        Memory<byte> buffer = new byte[4 * 1024];

        while (true)
        {
            await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            if (webSocket.CloseStatus.HasValue)
            {
                await stoppingTokenSource.CancelAsync();
                await sendTask;

                await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription,
                    CancellationToken.None);
                break;
            }
        }
    }

    private async Task SendTime(WebSocket webSocket, CancellationToken stoppingToken)
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

    private static ReadOnlyMemory<byte> GenerateMessage()
    {
        TimeResponse response = new() { Now = TimeService.Now.ToUnixTimeSeconds() };

        return JsonSerializer.SerializeToUtf8Bytes(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
