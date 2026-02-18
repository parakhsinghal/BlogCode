using Polly;
using Polly.Registry;
//using Polly.RateLimit;
using Polly.RateLimiting;
using System.Threading.RateLimiting;

namespace Consumer.ResilienceStrategies
{
    public class PollyStrategies
    {
        // Make all the properties settable only from PollyStrategies class, and gettable from outside
        ConcurrencyLimiterOptions concurrencyRateLimiterOptions;
        public ResiliencePipeline ConcurrencyRateLimiterStrategy { private set; get; }
        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { private set; get; }

        /// <summary>
        /// The method to initialize rate limiter options
        /// </summary>
        private void InitializeOptions()
        {
            concurrencyRateLimiterOptions = new ConcurrencyLimiterOptions
            {
                PermitLimit = 3, // Maximum number of concurrent executions
                QueueLimit = 10, // No queuing, requests will be rejected immediately if the limit is reached
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst // Process requests in the order they were received
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
        }

        public PollyStrategies()
        {
            InitializeOptions();
            InitializePipelines();
            RegisterPipelines();
        }
    }
}
