using Microsoft.AspNetCore.Mvc;
using PushServiceNet.Services;

namespace PushServiceNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SSEController : ControllerBase
    {
        private readonly SSEService _sseService;
        private readonly ILogger<SSEController> _logger;

        public SSEController(SSEService sseService, ILogger<SSEController> logger)
        {
            _sseService = sseService;
            _logger = logger;
        }

        [HttpGet("connect/{userId}")]
        public async Task Connect(string userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SSE connection request for user {UserId}", userId);

            Response.Headers.Add("X-Accel-Buffering", "no");

            try
            {
                await _sseService.HandleSSEConnectionAsync(HttpContext, userId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling SSE connection for user {UserId}", userId);
            }
        }
    }
}