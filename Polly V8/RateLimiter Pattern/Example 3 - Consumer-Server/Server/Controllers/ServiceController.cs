using Microsoft.AspNetCore.Mvc;
using ResilienceStrategies;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : Controller
    {
        private PollyStrategies pollyStrategies;

        public ServiceController(PollyStrategies pollyStrategies)
        {
            this.pollyStrategies = pollyStrategies;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> ServiceEndpoint(int id)
        {
            await pollyStrategies.StrategyPipelineRegistry.GetPipeline("ConcurrencyLimiterStrategy").Execute(async () =>
            {
                await Console.Out.WriteLineAsync($"Executing the action within the rate limiter strategy.{id}");

                // Simulate some work
                await Task.Delay(1000);
            });
            return Ok("Call succeeded.");
        }
    }
}
