using Consumer.ResilienceStrategies;
using Microsoft.AspNetCore.Mvc;
using Polly.Timeout;
using System.Net;

namespace Consumer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly PollyStrategies pollyStrategies;

        public ConsumerController(IHttpClientFactory _httpclientFactory, PollyStrategies _pollyStrategies)
        {
            httpClientFactory = _httpclientFactory;
            pollyStrategies = _pollyStrategies;
        }

        public async Task<IActionResult> ConsumerEndPoint()
        {
            string url = "http://localhost:5106/api/service";

            HttpClient client = httpClientFactory.CreateClient();
            client.Timeout = Timeout.InfiniteTimeSpan; // Explicitly mentioning that the http client should not timeout on its own

            try
            {
                var response = pollyStrategies.StrategyPipelineRegistry.GetPipeline("Timeout")
                                          .ExecuteAsync(async cancellationToken =>
                                          {
                                              return await client.GetAsync(url, cancellationToken);
                                          });

                string content = await response.Result.Content.ReadAsStringAsync();
                return Ok(content);
            }
            catch (TimeoutRejectedException ex)
            {
                return StatusCode((int)HttpStatusCode.RequestTimeout, ex.Message);                 
            }            
        }
    }
}

