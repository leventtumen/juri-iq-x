using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JuriIQ.Scheduler;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JuriIQ Scheduler service started at: {Time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("JuriIQ Scheduler service stopping at: {Time}", DateTimeOffset.Now);
    }
}
