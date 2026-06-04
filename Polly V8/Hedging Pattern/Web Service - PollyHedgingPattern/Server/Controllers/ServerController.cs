using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PollyStrategies;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly IHttpClientFactory httpClientFactory;
        private Strategies strategies;

        public ServerController(IHttpClientFactory httpClientFactory, Strategies strategies)
        {
            this.httpClientFactory = httpClientFactory;
            this.strategies = strategies;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string primaryServiceURL = @"http://localhost:5258";
            string serviceEndPoint = @"/api/Randomvalue";

            var httpClient = httpClientFactory.CreateClient("PrimaryService");
            httpClient.BaseAddress = new Uri(primaryServiceURL);

            var response = await strategies.HedgingStrategy.ExecuteAsync(async ct => await httpClient.GetAsync(primaryServiceURL + serviceEndPoint, ct),
                                                                         CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadAsStringAsync();
                return Ok(value);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to get a successful response from PrimaryService or BackupService.");
            }
        }        
    }
}
