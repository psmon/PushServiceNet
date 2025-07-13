using Akka.Actor;
using PushServiceNet.Models;

namespace PushServiceNet.Services
{
    public class TopicService
    {
        private readonly AkkaService _akkaService;
        private readonly IActorRef _topicManagerActor;

        public TopicService(AkkaService akkaService)
        {
            _akkaService = akkaService;
            _topicManagerActor = _akkaService.GetOrCreateActor<Actors.TopicManagerActor>("topic-manager");
        }

        public async Task<object> SubscribeUserToTopicAsync(string userId, string topicName)
        {
            var msg = new SubscribeToTopic { UserId = userId, TopicName = topicName };
            return await _topicManagerActor.Ask(msg, TimeSpan.FromSeconds(5));
        }

        public async Task<object> UnsubscribeUserFromTopicAsync(string userId, string topicName)
        {
            var msg = new UnsubscribeFromTopic { UserId = userId, TopicName = topicName };
            return await _topicManagerActor.Ask(msg, TimeSpan.FromSeconds(5));
        }

        public async Task<object> PublishMessageToTopicAsync(string topicName, string content, string? senderId = null)
        {
            var message = new TopicMessage
            {
                TopicName = topicName,
                Content = content,
                SenderId = senderId
            };

            var msg = new PublishToTopic { Message = message };
            return await _topicManagerActor.Ask(msg, TimeSpan.FromSeconds(5));
        }

        public async Task<IActorRef> GetUserActorAsync(string userId)
        {
            var msg = new RegisterUser { UserId = userId };
            return await _topicManagerActor.Ask<IActorRef>(msg, TimeSpan.FromSeconds(5));
        }

        public async Task<UserHistoryResponse> GetUserHistoryAsync(string userId)
        {
            var userActor = await GetUserActorAsync(userId);
            var msg = new GetUserHistory { UserId = userId };
            return await userActor.Ask<UserHistoryResponse>(msg, TimeSpan.FromSeconds(5));
        }
    }
}