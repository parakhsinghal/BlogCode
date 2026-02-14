using Polly.RateLimiting;
using RateLimiterPattern.Strategies;

namespace RateLimiterPattern
{
    public class MyClass
    {
        private PollyStrategies pollyStrategies; 
        
        public MyClass() 
        { 
            pollyStrategies = new PollyStrategies(); 
        }
        
        public async Task PrintStmt(int id)
        {
            try
            {
                await pollyStrategies.RateLimiterStrategy.ExecuteAsync(async cancellationToken =>
                {
                    await Console.Out.WriteLineAsync($"Executing the action within the rate limiter strategy.{id}");

                    // Simulate some work.
                    // This is necessary to see the effect of the rate limiter, as it will allow only a certain number of concurrent executions.
                    await Task.Delay(1000); 
                });
            }
            catch (RateLimiterRejectedException ex)
            {
                Console.WriteLine("Polly threw rate limiter exception");
            }
        }
    }
}
