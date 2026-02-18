using Microsoft.AspNetCore.Mvc;
using Polly;
using ResilienceStrategies;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;

namespace Consumer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private PollyStrategies pollyStrategies;

        public ConsumerController(IHttpClientFactory _httpclientFactory, PollyStrategies _pollyStrategies)
        {
            httpClientFactory = _httpclientFactory;
            pollyStrategies = _pollyStrategies;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> ConsumerEndPoint()
        {
            string url = "http://localhost:5106/api/service";

            HttpClient client = httpClientFactory.CreateClient();

            ConcurrentQueue<string> result = new ConcurrentQueue<string>();


            // Multiple requests are sent to the server to see the retry strategy in action,
            // and the results are collected in a concurrent queue to be returned in the response
            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (i, cancellationToken) =>
            {
                // Use a callback that accepts a CancellationToken and returns a ValueTask<HttpResponseMessage>,
                // and pass the loop's cancellationToken as the second parameter so the correct overload is selected.
                HttpResponseMessage response = await client.GetAsync(url + $"/{i}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Request id :{i} succeeded");
                    result.Enqueue($"Request ID: {i} Server responded");
                }
                else
                {
                    Console.WriteLine($"Request id :{i} failed at server");
                    result.Enqueue($"Request ID: {i} Problem happened with the request: {response.StatusCode}");
                }
            });


            StringBuilder responseData = new();

            foreach (string item in result.ToArray())
            {
                responseData.AppendLine(item);
            }

            return Ok(responseData.ToString());
        }

        [HttpGet]
        [Route("WithRetry")]
        public async Task<IActionResult> ConsumerEndPointWithRetry()
        {
            string url = "http://localhost:5106/api/service";

            HttpClient client = httpClientFactory.CreateClient();

            ConcurrentQueue<string> result = new ConcurrentQueue<string>();


            // Multiple requests are sent to the server to see the retry strategy in action,
            // and the results are collected in a concurrent queue to be returned in the response
            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (i, cancellationToken) =>
            {
                ResiliencePipeline<HttpResponseMessage>? retryPipeline;
                pollyStrategies.StrategyPipelineRegistry.TryGetPipeline<HttpResponseMessage>("RetryStrategy", out retryPipeline);

                if (retryPipeline is not null)
                {
                    // Use a callback that accepts a CancellationToken and returns a ValueTask<HttpResponseMessage>,
                    // and pass the loop's cancellationToken as the second parameter so the correct overload is selected.
                    HttpResponseMessage response = await retryPipeline.ExecuteAsync<HttpResponseMessage>(
                        ct => new ValueTask<HttpResponseMessage>(client.GetAsync(url + $"/{i}", ct)),
                        cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Request id :{i} succeeded");
                        result.Enqueue($"Request ID: {i} Server responded");
                    }
                    else
                    {
                        Console.WriteLine($"Request id :{i} failed at server");
                        result.Enqueue($"Request ID: {i} Problem happened with the request: {response.StatusCode}");
                    }
                }
            });


            StringBuilder responseData = new();

            foreach (string item in result.ToArray())
            {
                responseData.AppendLine(item);
            }

            return Ok(responseData.ToString());
        }
    }
}

