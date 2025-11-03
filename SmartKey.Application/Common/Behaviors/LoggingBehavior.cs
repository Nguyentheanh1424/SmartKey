using MediatR;
using Microsoft.Extensions.Logging;

namespace SmartKey.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {RequestName} with payload: {@Request}", typeof(TRequest).Name, request);

            var response = next();
            _logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);

            return response;
        }
    }
}
