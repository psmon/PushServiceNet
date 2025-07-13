using Akka.Actor;
using PushServiceNet.Models;

namespace PushServiceNet.Actors
{
    public class UserTopicActor : ReceiveActor
    {
        private readonly string _userId;
        private readonly HashSet<string> _subscribedTopics = new HashSet<string>();
        private readonly Queue<TopicMessage> _messageHistory = new Queue<TopicMessage>();
        private readonly Queue<TopicMessage> _pendingMessages = new Queue<TopicMessage>();
        private const int MaxHistorySize = 100;

        public UserTopicActor(string userId)
        {
            _userId = userId;

            Receive<SubscribeToTopic>(HandleSubscribeToTopic);
            Receive<UnsubscribeFromTopic>(HandleUnsubscribeFromTopic);
            Receive<TopicMessage>(HandleTopicMessage);
            Receive<GetUserHistory>(HandleGetUserHistory);
            Receive<CheckForMessages>(HandleCheckForMessages);
        }

        private void HandleSubscribeToTopic(SubscribeToTopic msg)
        {
            _subscribedTopics.Add(msg.TopicName);
        }

        private void HandleUnsubscribeFromTopic(UnsubscribeFromTopic msg)
        {
            _subscribedTopics.Remove(msg.TopicName);
        }

        private void HandleTopicMessage(TopicMessage msg)
        {
            if (_subscribedTopics.Contains(msg.TopicName))
            {
                _pendingMessages.Enqueue(msg);
                _messageHistory.Enqueue(msg);

                while (_messageHistory.Count > MaxHistorySize)
                {
                    _messageHistory.Dequeue();
                }
            }
        }

        private void HandleGetUserHistory(GetUserHistory msg)
        {
            var response = new UserHistoryResponse
            {
                Messages = _messageHistory.ToList()
            };
            Sender.Tell(response);
        }

        private void HandleCheckForMessages(CheckForMessages msg)
        {
            if (_pendingMessages.Count > 0)
            {
                var message = _pendingMessages.Dequeue();
                Sender.Tell(message);
            }
            else
            {
                Sender.Tell(new HeartbeatNotification());
            }
        }
    }
}