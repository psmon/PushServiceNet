namespace PushServiceNet.Models
{
    public class Topic
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public HashSet<string> Subscribers { get; set; } = new HashSet<string>();
    }
}