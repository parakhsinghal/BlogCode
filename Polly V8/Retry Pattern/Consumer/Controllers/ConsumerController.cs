﻿using Consumer.ResilienceStrategies;
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

            //HttpResponseMessage response = pollyStrategies
            //                               .StrategyPipelineRegistry.GetPipeline<HttpResponseMessage>("ImmediateRetry")
            //                               .Execute(() => client.GetAsync(url).Result);

            HttpResponseMessage response = pollyStrategies
                                           .StrategyPipelineRegistry.GetPipeline<HttpResponseMessage>("ConstantRetry")
                                           .Execute(() => client.GetAsync(url).Result);

            //HttpResponseMessage response = pollyStrategies
            //                               .StrategyPipelineRegistry.GetPipeline<HttpResponseMessage>("WaitAndRetry")
            //                               .Execute(() => client.GetAsync(url).Result);

            //HttpResponseMessage response = pollyStrategies
            //                               .StrategyPipelineRegistry.GetPipeline<HttpResponseMessage>("ExponentialRetry")
            //                               .Execute(() => client.GetAsync(url).Result);

            //HttpResponseMessage response = pollyStrategies.ImmediateRetryStrategyAsync.Execute<HttpResponseMessage>(() =>  client.GetAsync(url).Result);

            //HttpResponseMessage response = pollyStrategies.WaitAndRetryStrategyAsync.Execute(() => client.GetAsync(url).Result);

            //HttpResponseMessage response = pollyStrategies.ExponentialWaitRetryStrategyAsync.Execute(() => client.GetAsync(url).Result);

            //HttpResponseMessage response = client.GetAsync(url).Result; 

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

