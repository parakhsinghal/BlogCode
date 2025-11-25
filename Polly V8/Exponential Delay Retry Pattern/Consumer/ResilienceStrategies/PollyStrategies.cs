using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using System.Net;
using Polly.Registry;

namespace Consumer.ResilienceStrategies
{
    public class PollyStrategies
    {
        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { get; private set; }

        public ResiliencePipeline<HttpResponseMessage>? ExponentialWaitRetryStrategy { get; set; }
        private RetryStrategyOptions<HttpResponseMessage>? exponentialWaitRetryStrategyOptions;

        HttpStatusCode[] httpStatusCodesWorthRetrying = new HttpStatusCode[] {
                                                           HttpStatusCode.RequestTimeout,// 408
                                                           HttpStatusCode.InternalServerError, // 500
                                                           HttpStatusCode.BadGateway, // 502
                                                           HttpStatusCode.ServiceUnavailable, // 503
                                                           HttpStatusCode.GatewayTimeout // 504
                                                          };        

        private void InitializeOptions()
        {
            exponentialWaitRetryStrategyOptions = new RetryStrategyOptions<HttpResponseMessage>()
            {
                MaxRetryAttempts = 10,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                   .HandleResult(response => httpStatusCodesWorthRetrying.Contains(response.StatusCode))
                                   .Handle<HttpRequestException>()
                                   .Handle<TimeoutRejectedException>(),
                OnRetry = async args => { await Console.Out.WriteLineAsync("ExponentialRetry - Retrying call..."); }
            };
        }

        private void InitializePipelines()
        {           
            ExponentialWaitRetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry<HttpResponseMessage>(exponentialWaitRetryStrategyOptions).Build();
        }

        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new ResiliencePipelineRegistry<string>();

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("ExponentialRetry", (builder, context) =>
            {
                builder.AddPipeline(ExponentialWaitRetryStrategy);
            });
        }

        public PollyStrategies()
        {
            InitializeOptions();
            InitializePipelines();
            RegisterPipelines();
        }
    }
}
