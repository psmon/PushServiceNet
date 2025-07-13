namespace PushServiceNet.Models
{
    public class SSENotification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Event { get; set; } = "message";
        public string Data { get; set; } = string.Empty;
        public string? TopicName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class HeartbeatNotification : SSENotification
    {
        public HeartbeatNotification()
        {
            Event = "heartbeat";
            Data = "ping";
        }
    }
}