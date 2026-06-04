using Polly;
using Polly.Hedging;
using Polly.Registry;
using System.Net;

namespace PollyStrategies
{
    public class Strategies
    {
        // Make all the properties settable only from PollyStrategies class, and gettable from outside
        HedgingStrategyOptions<HttpResponseMessage> hedgingStrategyOptions;
        public ResiliencePipeline<HttpResponseMessage> HedgingStrategy { private set; get; }

        public ResiliencePipelineRegistry<string> StrategyPipelineRegistry { private set; get; }

        /// <summary>
        /// The method to initialize rate limiter options
        /// </summary>
        private void InitializeOptions()
        {
            hedgingStrategyOptions = new HedgingStrategyOptions<HttpResponseMessage>()
            {
                // Max number of hedged attempts to make (1 original + n hedged attempts)
                MaxHedgedAttempts = 2,

                // The delay between the original call and the hedged call.
                // This is the time we wait before starting a hedged call, hoping that the original call
                // would have completed by then. If the original call completes within this delay, no hedged call will be made.
                Delay = TimeSpan.FromSeconds(1),

                // The predicate to determine whether a response should trigger hedging or not.
                // In this case, we are checking if the response does not contain "completed at"
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                   .HandleResult(response => response.StatusCode != HttpStatusCode.OK)
                                   .Handle<HttpRequestException>(),

                // The action to perform when a hedged call is made.
                // This is where you can log the hedging event, for example.
                OnHedging = async args =>
                {
                    await Console.Out.WriteLineAsync("Hedging Strategy kicked in - Hedging call...");
                },

                // ActionGenerator: called by Polly when it decides to hedge.
                // Must return a Func that Polly can invoke — do NOT await here,
                // just return the delegate. Polly calls it at the right moment.
                ActionGenerator = args =>
                {
                    return async () =>
                    {
                        try
                        {
                            HttpClient backupClient = new HttpClient();

                            // Backup service details are owned here, not in the controller
                            string backupServiceURL = "http://localhost:5071";
                            string serviceEndPoint = "/api/Randomvalue";

                            // Create a fresh HttpClient for the backup service.
                            // Use a named client so BaseAddress and timeouts
                            // are configured once in Program.cs, not here.                            
                            backupClient.BaseAddress = new Uri(backupServiceURL);

                            var backupResponse = await backupClient.GetAsync(
                                backupServiceURL + serviceEndPoint
                            );

                            // Wrap the HttpResponseMessage in an Outcome so Polly
                            // can evaluate it against ShouldHandle
                            return Outcome.FromResult(backupResponse);
                        }
                        catch (Exception ex)
                        {
                            // Wrap exceptions too — lets ShouldHandle catch them
                            // and potentially trigger another hedge attempt
                            return Outcome.FromException<HttpResponseMessage>(ex);
                        }
                    };
                }
            };



        }
        /// <summary>
        /// The method to initialize a pipeline with a strategy or a combination thereof
        /// </summary>
        private void InitializePipelines()
        {
            HedgingStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>()
                                  .AddHedging(hedgingStrategyOptions)
                                  .Build();
        }

        /// <summary>
        /// The method to register pipelines in the registry with a key for retrieval
        /// </summary>
        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new();

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("HedgingStrategy", (builder, context) =>
            {
                builder.AddPipeline(HedgingStrategy);
            });
        }

        public Strategies()
        {
            InitializeOptions();
            InitializePipelines();
            RegisterPipelines();
        }

    }
}
