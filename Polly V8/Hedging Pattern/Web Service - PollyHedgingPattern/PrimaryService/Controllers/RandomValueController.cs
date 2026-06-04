using Microsoft.AspNetCore.Mvc;

namespace PrimaryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RandomValueController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetRandomValue()
        {
            // Generate a random delay between 500ms and 2000ms to simulate processing time
            int randomDelay = Random.Shared.Next(500, 2000);

            await Task.Delay(randomDelay);

            // Generate a random number between 1 and 100 to return as the response
            int randomValue = Random.Shared.Next(1, 101); 

            return Ok(randomValue);
        }
    }
}
