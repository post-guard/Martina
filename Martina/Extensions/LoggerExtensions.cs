using Martina.Models;

namespace Martina.Extensions;

public static class LoggerExtensions
{
    public static void LogServiceQueue<T>(this ILogger<T> logger, string message,
        IEnumerable<AirConditionerService> services)
    {
        IEnumerable<string> roomNames = services.Select(s => s.Room.RoomName);

        logger.LogInformation("{} {}", message, string.Join(',', roomNames));
    }
}
