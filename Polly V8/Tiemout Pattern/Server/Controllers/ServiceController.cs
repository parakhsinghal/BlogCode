using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : Controller
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> ServiceEndpoint()
        {
            Random random = new Random();
            int randomDelay = random.Next(10, 100) * 1000; // Generate a random delay between 10 and 100 seconds

            await Task.Delay(randomDelay);
            return Ok("Call succeeded.");
        }
    }
}
