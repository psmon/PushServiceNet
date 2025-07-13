using System.Text.Json;
using Akka.Actor;
using PushServiceNet.Models;

namespace PushServiceNet.Services
{
    public class SSEService
    {
        private readonly TopicService _topicService;
        private readonly ILogger<SSEService> _logger;

        public SSEService(TopicService topicService, ILogger<SSEService> logger)
        {
            _topicService = topicService;
            _logger = logger;
        }

        public async Task HandleSSEConnectionAsync(HttpContext context, string userId, CancellationToken cancellationToken)
        {
            context.Response.Headers.Add("Content-Type", "text/event-stream");
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("Connection", "keep-alive");

            await context.Response.WriteAsync($"data: {{\"event\":\"connected\",\"userId\":\"{userId}\"}}\n\n");
            await context.Response.Body.FlushAsync();

            var userActor = await _topicService.GetUserActorAsync(userId);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var checkMsg = new CheckForMessages { UserId = userId };
                    var response = await userActor.Ask(checkMsg, TimeSpan.FromSeconds(1));

                    if (response is TopicMessage topicMessage)
                    {
                        var sseData = new
                        {
                            id = topicMessage.Id,
                            @event = "message",
                            topic = topicMessage.TopicName,
                            data = topicMessage.Content,
                            timestamp = topicMessage.Timestamp,
                            senderId = topicMessage.SenderId
                        };

                        var json = JsonSerializer.Serialize(sseData);
                        await context.Response.WriteAsync($"id: {topicMessage.Id}\n");
                        await context.Response.WriteAsync($"event: message\n");
                        await context.Response.WriteAsync($"data: {json}\n\n");
                        await context.Response.Body.FlushAsync();
                    }
                    else if (response is HeartbeatNotification)
                    {
                        await context.Response.WriteAsync($"event: heartbeat\n");
                        await context.Response.WriteAsync($"data: {{\"timestamp\":\"{DateTime.UtcNow:O}\"}}\n\n");
                        await context.Response.Body.FlushAsync();
                    }

                    await Task.Delay(1000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SSE connection for user {UserId}", userId);
                    break;
                }
            }

            _logger.LogInformation("SSE connection closed for user {UserId}", userId);
        }
    }
}