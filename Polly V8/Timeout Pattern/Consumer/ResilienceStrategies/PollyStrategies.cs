using Polly;
using Polly.Registry;
using Polly.Timeout;

namespace Consumer.ResilienceStrategies
{
    public class PollyStrategies
    {
        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { get; private set; }

        public ResiliencePipeline TimeoutStrategy { get; set; }
        private TimeoutStrategyOptions timeoutStrategyOptions;

        private void InitializeOptions()
        {
            timeoutStrategyOptions = new TimeoutStrategyOptions()
            {
                Timeout = TimeSpan.FromSeconds(2), // The strategy has been programmed to timeout after 2 seconds                
                OnTimeout = async args => { await Console.Out.WriteLineAsync("Polly - Request timed out..."); }
            };
        }

        private void InitializePipelines()
        {
            try
            {
                TimeoutStrategy = new ResiliencePipelineBuilder().AddTimeout(timeoutStrategyOptions).Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new ResiliencePipelineRegistry<string>();

            StrategyPipelineRegistry.TryAddBuilder("Timeout", (builder, context) =>
            {
                builder.AddPipeline(TimeoutStrategy);
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
