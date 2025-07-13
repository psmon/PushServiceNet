namespace PushServiceNet.Models
{
    public class UserSubscription
    {
        public string UserId { get; set; } = string.Empty;
        public HashSet<string> SubscribedTopics { get; set; } = new HashSet<string>();
        public DateTime LastConnected { get; set; } = DateTime.UtcNow;
    }
}