using Polly;
using Polly.Registry;
using System.Threading.RateLimiting;

namespace RateLimiterPattern.Strategies
{
    public class PollyStrategies
    {
        // Make all the properties settable only from PollyStrategies class, and gettable from outside
        ConcurrencyLimiterOptions concurrencyRateLimiterOptions;
        public ResiliencePipeline RateLimiterStrategy { private set; get; }
        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { private set; get; }

        /// <summary>
        /// The method to initialize rate limiter options
        /// </summary>
        private void InitializeOptions()
        {
            concurrencyRateLimiterOptions = new ConcurrencyLimiterOptions
            {
                PermitLimit = 3, // Maximum number of concurrent executions
                QueueLimit = 1, // Play with this limit to see the effect of queuing requests when the permit limit is reached
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst // Process requests in the order they were received
            };
        }
        /// <summary>
        /// The method to initialize a pipeline with a strategy or a combination thereof
        /// </summary>
        private void InitializePipelines()
        {
            RateLimiterStrategy = new ResiliencePipelineBuilder()
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
                                                builder.AddPipeline(RateLimiterStrategy);
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
