using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace ResilienceDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResilienceController : ControllerBase
    {
        IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ResilienceController> _logger;

        public ResilienceController(ILogger<ResilienceController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("calling-long-response-operation")]
        public async Task<IActionResult> CallLongRunningService()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var tasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 30; i++)
            {
                _logger.LogDebug($"Making request number {i + 1}");
                var task = httpClient.GetAsync("http://localhost:5163/data");
                tasks.Add(task);
            }

            _logger.LogDebug("Waiting...");
            await Task.WhenAll(tasks);
            _logger.LogDebug("Done!");
            return Ok();
        }

        [HttpGet("retry")]
        public async Task<IActionResult> PerformRetry()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync("http://localhost:5163/data/error");
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }

            return StatusCode(500);
        }

        [HttpGet("bulkhead")]
        public async Task<IActionResult> PerformBulkHead()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < 10; i++)
            {
                var task = httpClient.GetAsync("http://localhost:5163/data/error");
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            return Ok();
        }

        [HttpGet("timeout")]
        public async Task<IActionResult> Timeout()
        {
            var httpClient = _httpClientFactory.CreateClient();
            await httpClient.GetAsync("http://localhost:5163/data");
            return Ok();
        }

        [HttpGet("circuit-breaker")]
        public async Task<IActionResult> CircuitBreaker()
        {
            var httpClient = _httpClientFactory.CreateClient();
            for(int i = 0; i< 10; i++)
            {
                await httpClient.GetAsync("http://localhost:5163/data/error");
            }

            return Ok();
        }
    }
}