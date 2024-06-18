using Martina.Models;

namespace Martina.Extensions;

public static class LoggerExtensions
{
    /// <summary>
    /// 输出当前队列中的房间信息
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message">输出的提示信息</param>
    /// <param name="services">房间列表</param>
    /// <typeparam name="T"></typeparam>
    public static void LogServiceQueue<T>(this ILogger<T> logger, string message,
        IEnumerable<AirConditionerService> services)
    {
        IEnumerable<string> roomNames = services.Select(s => $"{s.Room.RoomName}-{s.TimeToLive}");

        logger.LogDebug("{} {}", message, string.Join(',', roomNames));
    }
}
