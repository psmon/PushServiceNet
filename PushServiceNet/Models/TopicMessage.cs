namespace PushServiceNet.Models
{
    public class TopicMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TopicName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? SenderId { get; set; }
    }
}