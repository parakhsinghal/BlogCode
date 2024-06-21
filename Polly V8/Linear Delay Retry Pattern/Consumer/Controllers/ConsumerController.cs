using Consumer.ResilienceStrategies;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult ConsumerEndPoint()
        {
            string url = "http://localhost:5106/api/service";

            HttpClient client = httpClientFactory.CreateClient();

            HttpResponseMessage response = pollyStrategies
                                           .StrategyPipelineRegistry.GetPipeline<HttpResponseMessage>("LinearWaitAndRetry")
                                           .Execute(() => client.GetAsync(url).Result);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return Ok("Server responded");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Problem happened with the request")  ;
            }
        }
    }
}

