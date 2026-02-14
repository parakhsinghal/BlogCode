using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Consumer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ConsumerController(IHttpClientFactory _httpclientFactory)
        {
            httpClientFactory = _httpclientFactory;
        }

        public async Task<IActionResult> ConsumerEndPoint()
        {
            string url = "http://localhost:5106/api/service";

            HttpClient client = httpClientFactory.CreateClient();

            ConcurrentQueue<string> result = new ConcurrentQueue<string>();

            await Parallel.ForEachAsync(Enumerable.Range(0, 10), async (i, cancellationToken) =>
            {
                HttpResponseMessage response = await client.GetAsync(url + $"/{i}", cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
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
                responseData.AppendLine( item ); 
            }
           
            return Ok(responseData.ToString());
        }
    }
}

