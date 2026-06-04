using Polly;
using Polly.Hedging;
using Polly.Registry;

namespace PollyHedgingDemo
{
    public class PollyStrategies
    {
        private readonly Services services;
        public int taskId;

        public PollyStrategies(int taskId, Services services)
        {
            this.taskId = taskId;
            this.services = services;
            InitializeOptions();
            InitializePipelines();
            RegisterPipelines();
        }

        // Make all the properties settable only from PollyStrategies class, and gettable from outside
        HedgingStrategyOptions<string> hedgingStrategyOptions;
        public ResiliencePipeline<string> HedgingStrategy { private set; get; }

        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { private set; get; }

        /// <summary>
        /// The method to initialize rate limiter options
        /// </summary>
        private void InitializeOptions()
        {
            hedgingStrategyOptions = new HedgingStrategyOptions<string>()
            {
                // Max number of hedged attempts to make (1 original + n hedged attempts)
                MaxHedgedAttempts = 2,

                // The delay between the original call and the hedged call.
                // This is the time we wait before starting a hedged call, hoping that the original call
                // would have completed by then. If the original call completes within this delay, no hedged call will be made.
                Delay = TimeSpan.FromSeconds(1),

                // The predicate to determine whether a response should trigger hedging or not.
                // In this case, we are checking if the response does not contain "completed at"
                ShouldHandle = new PredicateBuilder<string>()
                                   .HandleResult(response => !response.Contains("completed at"))
                                   .Handle<Exception>(),

                // The action to perform when a hedged call is made.
                // This is where you can log the hedging event, for example.
                OnHedging =async args => 
                                        { 
                                           await Console.Out.WriteLineAsync("Hedging Strategy kicked in - Hedging call..."); 
                                        },

                ActionGenerator = args =>
                {
                    // args.AttemptNumber: 0 = original, 1 = first hedge, 2 = second hedge, etc.
                    // BackupService returns Task<string>, but the ActionGenerator expects a Func<ValueTask<Outcome<string>>>.
                    // Return an async ValueTask lambda that awaits the Task<string> and wraps the string into an Outcome<string>.
                    return async ValueTask<Outcome<string>> () =>
                    {
                        var backupResult = await services.BackupService(taskId);
                        return Outcome.FromResult(backupResult);
                    };
                }
            };

          
            
        }
        /// <summary>
        /// The method to initialize a pipeline with a strategy or a combination thereof
        /// </summary>
        private void InitializePipelines()
        {
            HedgingStrategy = new ResiliencePipelineBuilder<string>()
                                  .AddHedging(hedgingStrategyOptions)
                                  .Build();
        }

        /// <summary>
        /// The method to register pipelines in the registry with a key for retrieval
        /// </summary>
        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new();

            StrategyPipelineRegistry.TryAddBuilder<string>("HedgingStrategy", (builder, context) =>
            {
                builder.AddPipeline(HedgingStrategy);
            });                       
        }

        

    }
}
