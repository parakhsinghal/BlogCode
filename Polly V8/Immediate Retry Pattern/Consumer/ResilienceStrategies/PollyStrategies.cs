using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using System.Net;

namespace Consumer.ResilienceStrategies
{
    public class PollyStrategies
    {
        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { get; private set; }        

        public ResiliencePipeline<HttpResponseMessage>? ImmediateRetryStrategy { get; private set; }
        private RetryStrategyOptions<HttpResponseMessage>? immediateRetryStrategyOptions;     

        HttpStatusCode[] httpStatusCodesWorthRetrying = new HttpStatusCode[] {
                                                           HttpStatusCode.RequestTimeout,// 408
                                                           HttpStatusCode.InternalServerError, // 500
                                                           HttpStatusCode.BadGateway, // 502
                                                           HttpStatusCode.ServiceUnavailable, // 503
                                                           HttpStatusCode.GatewayTimeout // 504
                                                        };        

        private void InitializeOptions()
        {
            immediateRetryStrategyOptions = new RetryStrategyOptions<HttpResponseMessage>()
            {
                MaxRetryAttempts = 10,
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                   .HandleResult(response => httpStatusCodesWorthRetrying.Contains(response.StatusCode))
                                   .Handle<HttpRequestException>()
                                   .Handle<TimeoutRejectedException>(),
                OnRetry = async args => { await Console.Out.WriteLineAsync("ImmediateRetry - Retrying call..."); }

            };        
        }

        private void InitializePipelines()
        {
            ImmediateRetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry<HttpResponseMessage>(immediateRetryStrategyOptions).Build();
            
        }

        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new ResiliencePipelineRegistry<string>();

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("ImmediateRetry", (builder, context) =>
            {
                builder.AddPipeline(ImmediateRetryStrategy);

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
