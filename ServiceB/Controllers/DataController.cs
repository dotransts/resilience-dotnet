using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AnotherService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;

        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            _logger.LogDebug("Received request");
            _logger.LogDebug("Request received from port {}", Request.HttpContext.Connection.RemotePort);
            await Task.Delay(TimeSpan.FromSeconds(30));

            return Ok();
        }

        [HttpGet("error")]
        public IActionResult ReturnsInternalServerError()
        {
            _logger.LogDebug("Received Request!");
            return StatusCode(500);
        }
    }
}
