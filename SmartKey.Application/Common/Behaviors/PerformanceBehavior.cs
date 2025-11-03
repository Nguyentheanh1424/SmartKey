using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SmartKey.Application.Common.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        private readonly Stopwatch _timer = new();

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _timer.Start();
            var response = next();
            _timer.Stop();



            if (_timer.ElapsedMilliseconds > 500) // Log nếu thời gian xử lý vượt quá 500ms
            {
                var requestName = typeof(TRequest).Name;
                _logger.LogWarning("Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds) {@Request}",
                    requestName, _timer.ElapsedMilliseconds, request);
            }

            return response;
        }
    }
}
