using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class CallApiBackgroundService : BackgroundService
{   
    private readonly ILogger<CallApiBackgroundService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CallApiBackgroundService(
        ILogger<CallApiBackgroundService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CallApiAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when calling API");
            }

            // Chờ 5 phút
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task CallApiAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();

        var response = await client.GetAsync("https://workflow-0euv.onrender.com/api/Ping", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("API response: {content}", content);
        }
        else
        {
            _logger.LogWarning("API call failed. Status: {status}", response.StatusCode);
        }
    }
}
