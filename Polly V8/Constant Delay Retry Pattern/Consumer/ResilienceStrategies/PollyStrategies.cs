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
        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { get; set; }
        public ResiliencePipeline<HttpResponseMessage>? ConstantDelayRetryStrategy { get; private set; }
        private RetryStrategyOptions<HttpResponseMessage>? constantDelayRetryStrategyOptions;        

        HttpStatusCode[] httpStatusCodesWorthRetrying = new HttpStatusCode[] {
                                                           HttpStatusCode.RequestTimeout,// 408
                                                           HttpStatusCode.InternalServerError, // 500
                                                           HttpStatusCode.BadGateway, // 502
                                                           HttpStatusCode.ServiceUnavailable, // 503
                                                           HttpStatusCode.GatewayTimeout // 504
                                                        };        

        private void InitializeOptions()
        {   
            constantDelayRetryStrategyOptions = new RetryStrategyOptions<HttpResponseMessage>()
            {
                MaxRetryAttempts = 10,
                BackoffType = DelayBackoffType.Constant,                
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                   .HandleResult(response => httpStatusCodesWorthRetrying.Contains(response.StatusCode))
                                   .Handle<HttpRequestException>()
                                   .Handle<TimeoutRejectedException>(),
                OnRetry = async args => { await Console.Out.WriteLineAsync("ConstantRetry - Retrying call..."); }
            };
        }

        private void InitializePipelines()
        {
            ConstantDelayRetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry<HttpResponseMessage>(constantDelayRetryStrategyOptions).Build();
        }

        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new ResiliencePipelineRegistry<string>();

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("ConstantRetry", (builder, context) =>
            {
                builder.AddPipeline(ConstantDelayRetryStrategy);
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
