namespace PushServiceNet.Models
{
    // Messages for actor communication
    public class SubscribeToTopic
    {
        public string UserId { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
    }

    public class UnsubscribeFromTopic
    {
        public string UserId { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
    }

    public class PublishToTopic
    {
        public TopicMessage Message { get; set; } = null!;
    }

    public class GetUserHistory
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class UserHistoryResponse
    {
        public List<TopicMessage> Messages { get; set; } = new List<TopicMessage>();
    }

    public class CheckForMessages
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class RegisterUser
    {
        public string UserId { get; set; } = string.Empty;
    }
}