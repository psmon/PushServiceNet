using Akka.Actor;
using PushServiceNet.Models;

namespace PushServiceNet.Actors
{
    public class TopicManagerActor : ReceiveActor
    {
        private readonly Dictionary<string, Topic> _topics = new Dictionary<string, Topic>();
        private readonly Dictionary<string, IActorRef> _userActors = new Dictionary<string, IActorRef>();

        public TopicManagerActor()
        {
            Receive<SubscribeToTopic>(HandleSubscribeToTopic);
            Receive<UnsubscribeFromTopic>(HandleUnsubscribeFromTopic);
            Receive<PublishToTopic>(HandlePublishToTopic);
            Receive<RegisterUser>(HandleRegisterUser);
        }

        private void HandleRegisterUser(RegisterUser msg)
        {
            if (!_userActors.ContainsKey(msg.UserId))
            {
                var userActor = Context.ActorOf(Props.Create<UserTopicActor>(msg.UserId), $"user-{msg.UserId}");
                _userActors[msg.UserId] = userActor;
            }
            Sender.Tell(_userActors[msg.UserId]);
        }

        private void HandleSubscribeToTopic(SubscribeToTopic msg)
        {
            if (!_topics.ContainsKey(msg.TopicName))
            {
                _topics[msg.TopicName] = new Topic { Name = msg.TopicName };
            }

            _topics[msg.TopicName].Subscribers.Add(msg.UserId);

            if (_userActors.TryGetValue(msg.UserId, out var userActor))
            {
                userActor.Tell(msg);
            }

            Sender.Tell(new { Success = true, Message = $"User {msg.UserId} subscribed to topic {msg.TopicName}" });
        }

        private void HandleUnsubscribeFromTopic(UnsubscribeFromTopic msg)
        {
            if (_topics.TryGetValue(msg.TopicName, out var topic))
            {
                topic.Subscribers.Remove(msg.UserId);

                if (_userActors.TryGetValue(msg.UserId, out var userActor))
                {
                    userActor.Tell(msg);
                }
            }

            Sender.Tell(new { Success = true, Message = $"User {msg.UserId} unsubscribed from topic {msg.TopicName}" });
        }

        private void HandlePublishToTopic(PublishToTopic msg)
        {
            if (_topics.TryGetValue(msg.Message.TopicName, out var topic))
            {
                foreach (var subscriberId in topic.Subscribers)
                {
                    if (_userActors.TryGetValue(subscriberId, out var userActor))
                    {
                        userActor.Tell(msg.Message);
                    }
                }

                Sender.Tell(new { Success = true, Message = $"Message published to topic {msg.Message.TopicName}", SubscriberCount = topic.Subscribers.Count });
            }
            else
            {
                Sender.Tell(new { Success = false, Message = $"Topic {msg.Message.TopicName} does not exist" });
            }
        }
    }
}