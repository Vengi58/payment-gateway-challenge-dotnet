using MediatR;

using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Text.Json;

namespace PaymentGateway.Application.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }


        //public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        //{
        //    Stopwatch stopwatch = new();
        //    _logger.LogInformation($"Handling {typeof(TRequest).Name}");
        //    stopwatch.Start();

        //    var response = await next();

        //    stopwatch.Stop();

        //    _logger.LogInformation($"Handled {typeof(TResponse).Name} in {stopwatch.ElapsedMilliseconds} ms");
        //    stopwatch.Reset();

        //    return response;
        //}

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = request.GetType().Name;
            var requestGuid = Guid.NewGuid().ToString();

            var requestNameWithGuid = $"{requestName} [{requestGuid}]";

            _logger.LogInformation($"[START] {requestNameWithGuid}");
            TResponse response;

            var stopwatch = Stopwatch.StartNew();
            try
            {
                try
                {
                    _logger.LogInformation($"[PROPS] {requestNameWithGuid} {JsonSerializer.Serialize(request)}");
                }
                catch (NotSupportedException)
                {
                    _logger.LogInformation($"[Serialization ERROR] {requestNameWithGuid} Could not serialize the request.");
                }

                response = await next();
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    $"[END] {requestNameWithGuid}; Execution time={stopwatch.ElapsedMilliseconds}ms");
            }

            return response;
        }
    }
}
