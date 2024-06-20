using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : Controller
    {
        [HttpGet]
        [Route("")]
        public IActionResult ServiceEndpoint()
        {
            Random random = new Random();
            int dice = random.Next(1, 100);

            if (dice < 30)
            {
                return Ok("Call succeeded. Dice rolled in your favour.");
            }
            else if (dice > 30 && dice < 50) 
            {
                return StatusCode(StatusCodes.Status502BadGateway, "Bad gateway.");
            }
            else if(dice > 50 && dice < 70)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
            else 
            {
                return StatusCode(StatusCodes.Status408RequestTimeout, "Request Timeout.");
            }
        }
    }
}
