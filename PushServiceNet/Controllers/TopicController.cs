using Microsoft.AspNetCore.Mvc;
using PushServiceNet.Services;

namespace PushServiceNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly TopicService _topicService;
        private readonly ILogger<TopicController> _logger;

        public TopicController(TopicService topicService, ILogger<TopicController> logger)
        {
            _topicService = topicService;
            _logger = logger;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
        {
            try
            {
                var result = await _topicService.SubscribeUserToTopicAsync(request.UserId, request.TopicName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing user {UserId} to topic {TopicName}", request.UserId, request.TopicName);
                return StatusCode(500, new { error = "Failed to subscribe to topic" });
            }
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
        {
            try
            {
                var result = await _topicService.UnsubscribeUserFromTopicAsync(request.UserId, request.TopicName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing user {UserId} from topic {TopicName}", request.UserId, request.TopicName);
                return StatusCode(500, new { error = "Failed to unsubscribe from topic" });
            }
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromBody] PublishMessageRequest request)
        {
            try
            {
                var result = await _topicService.PublishMessageToTopicAsync(request.TopicName, request.Content, request.SenderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to topic {TopicName}", request.TopicName);
                return StatusCode(500, new { error = "Failed to publish message" });
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetUserHistory(string userId)
        {
            try
            {
                var history = await _topicService.GetUserHistoryAsync(userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to get user history" });
            }
        }
    }

    public class SubscribeRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
    }

    public class UnsubscribeRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
    }

    public class PublishMessageRequest
    {
        public string TopicName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? SenderId { get; set; }
    }
}