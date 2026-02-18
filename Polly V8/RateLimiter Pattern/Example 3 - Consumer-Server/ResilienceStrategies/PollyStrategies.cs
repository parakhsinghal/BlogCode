using Polly;
using Polly.Registry;
using Polly.Retry;
using System.Net;
using System.Threading.RateLimiting;

namespace ResilienceStrategies
{
    public class PollyStrategies
    {
        // Make all the properties settable only from PollyStrategies class, and gettable from outside
        ConcurrencyLimiterOptions concurrencyRateLimiterOptions;  
        public ResiliencePipeline ConcurrencyRateLimiterStrategy { private set; get; }

        RetryStrategyOptions<HttpResponseMessage>? retryStrategyOptions;      
        public ResiliencePipeline<HttpResponseMessage> RetryStrategy { private set; get; }

        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { private set; get; }

        /// <summary>
        /// The method to initialize rate limiter options
        /// </summary>
        private void InitializeOptions()
        {
            concurrencyRateLimiterOptions = new ConcurrencyLimiterOptions
            {
                PermitLimit = 2, // Maximum number of concurrent executions
                QueueLimit = 2, // No queuing, requests will be rejected immediately if the limit is reached
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst // Process requests in the order they were received
            };

            retryStrategyOptions = new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 2,
                BackoffType = DelayBackoffType.Constant, // Use a constant backoff strategy
                Delay = TimeSpan.FromSeconds(1), // Delay between retries
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                   .HandleResult(response => response.StatusCode != HttpStatusCode.OK) // Basically anything that is not OK
                                   .Handle<HttpRequestException>(), // Handle any http exception
                OnRetry = async args => { await Console.Out.WriteLineAsync("ConstantRetry - Retrying call..."); }
            };
        }
        /// <summary>
        /// The method to initialize a pipeline with a strategy or a combination thereof
        /// </summary>
        private void InitializePipelines()
        {
            ConcurrencyRateLimiterStrategy = new ResiliencePipelineBuilder()
                                      .AddRateLimiter(new ConcurrencyLimiter(concurrencyRateLimiterOptions))
                                      .Build();

            RetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>()
                            .AddRetry<HttpResponseMessage>(retryStrategyOptions)
                            .Build();
        }

        /// <summary>
        /// The method to register pipelines in the registry with a key for retrieval
        /// </summary>
        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new();

            StrategyPipelineRegistry.TryAddBuilder("ConcurrencyLimiterStrategy", (builder, context) =>
            {
                builder.AddPipeline(ConcurrencyRateLimiterStrategy);                
            });

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("RetryStrategy", (builder, context) =>
            {
                builder.AddPipeline(RetryStrategy);
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
