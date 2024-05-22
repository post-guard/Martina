using Martina.Models;
using Microsoft.Extensions.Options;

namespace Martina.Services;

public sealed class TimeService(IOptions<TimeOption> timeOption) : BackgroundService, IDisposable
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));

    public static DateTimeOffset Now { get; private set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Now = DateTimeOffset.Now;

        try
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                Now = Now.AddSeconds((double)timeOption.Value.Factor);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public override void Dispose() => _timer.Dispose();
}
