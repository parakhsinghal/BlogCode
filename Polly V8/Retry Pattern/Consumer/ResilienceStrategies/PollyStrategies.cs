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

        public ResiliencePipeline<HttpResponseMessage>? ImmediateRetryStrategy { get; private set; }
        private RetryStrategyOptions<HttpResponseMessage>? immediateRetryStrategyOptions;

        public ResiliencePipeline<HttpResponseMessage>? ConstantDelayRetryStrategy { get; private set; }
        private RetryStrategyOptions<HttpResponseMessage>? constantDelayRetryStrategyOptions;

        public ResiliencePipeline<HttpResponseMessage>? LinearWaitAndRetryStrategy { get; set; }
        private RetryStrategyOptions<HttpResponseMessage>? linearWaitAndRetryStrategyOptions;

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

            linearWaitAndRetryStrategyOptions = new RetryStrategyOptions<HttpResponseMessage>()
            {
                MaxRetryAttempts = 10,
                BackoffType = DelayBackoffType.Linear,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                   .HandleResult(response => httpStatusCodesWorthRetrying.Contains(response.StatusCode))
                                   .Handle<HttpRequestException>()
                                   .Handle<TimeoutRejectedException>(),
                OnRetry = async args => { await Console.Out.WriteLineAsync("WaitAndRetry - Retrying call..."); }
            };

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
            ImmediateRetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry<HttpResponseMessage>(immediateRetryStrategyAsyncOptions).Build();
            ConstantDelayRetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry<HttpResponseMessage>(constantDelayRetryStrategyAsyncOptions).Build();
            LinearWaitAndRetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry<HttpResponseMessage>(linearWaitAndRetryStrategyAsyncOptions).Build();
            ExponentialWaitRetryStrategy = new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry<HttpResponseMessage>(exponentialWaitRetryStrategyAsyncOptions).Build();
        }

        private void RegisterPipelines()
        {
            StrategyPipelineRegistry = new ResiliencePipelineRegistry<string>();

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("ImmediateRetry", (builder, context) =>
            {
                builder.AddPipeline(ImmediateRetryStrategy);

            });

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("ConstantRetry", (builder, context) =>
            {
                builder.AddPipeline(ConstantDelayRetryStrategy);

            });

            StrategyPipelineRegistry.TryAddBuilder<HttpResponseMessage>("WaitAndRetry", (builder, context) =>
            {
                builder.AddPipeline(LinearWaitAndRetryStrategy);

            });

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
