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
       
        public ResiliencePipeline<HttpResponseMessage>? LinearWaitAndRetryStrategy { get; set; }
        private RetryStrategyOptions<HttpResponseMessage>? linearWaitAndRetryStrategyOptions;

        HttpStatusCode[] httpStatusCodesWorthRetrying = new HttpStatusCode[] {
                                                           HttpStatusCode.RequestTimeout,// 408
                                                           HttpStatusCode.InternalServerError, // 500
                                                           HttpStatusCode.BadGateway, // 502
                                                           HttpStatusCode.ServiceUnavailable, // 503
                                                           HttpStatusCode.GatewayTimeout // 504
                                                        };        

        private void InitializeOptions()
        {
            linearWaitAndRetryStrategyOptions = new RetryStrategyOptions<HttpResponseMessage>()
            {
                MaxRetryAttempts = 10,
                BackoffType = DelayBackoffType.Linear,
                Delay = TimeSpan.FromSeconds(1), //This will introduce a linear delay of 1 seconds
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                   .HandleResult(response => httpStatusCodesWorthRetrying.Contains(response.StatusCode))
                                   .Handle<HttpRequestException>()
                                   .Handle<TimeoutRejectedException>(),
                OnRetry = async args => { 
                    // This will print the notification about  retry call and corresponding in-between delay
                    await Console.Out.WriteLineAsync($"\nLinearWaitAndRetry - Retry call. Delay: {args.RetryDelay.TotalSeconds.ToString()}\n");                
                }
            };
        }

        private void InitializePipelines()
        {
            LinearWaitAndRetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry<HttpResponseMessage>(linearWaitAndRetryStrategyOptions).Build();
        }

        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new ResiliencePipelineRegistry<string>();            

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("LinearWaitAndRetry", (builder, context) =>
            {
                builder.AddPipeline(LinearWaitAndRetryStrategy);

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
