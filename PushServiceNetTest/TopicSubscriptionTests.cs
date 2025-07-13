using Xunit;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using PushServiceNet.Actors;
using PushServiceNet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PushServiceNetTest
{
    public class TopicSubscriptionTests : TestKit
    {
        [Fact]
        public async Task User1_SubscribesToTopicA_User2_SubscribesToTopicB_OnlyUser1_ReceivesTopicA_Messages()
        {
            // Arrange
            var topicManager = Sys.ActorOf(Props.Create<TopicManagerActor>(), "topic-manager");
            
            // Register users
            var user1Actor = await topicManager.Ask<IActorRef>(new RegisterUser { UserId = "user1" });
            var user2Actor = await topicManager.Ask<IActorRef>(new RegisterUser { UserId = "user2" });

            // Subscribe users to different topics
            await topicManager.Ask(new SubscribeToTopic { UserId = "user1", TopicName = "topic-a" });
            await topicManager.Ask(new SubscribeToTopic { UserId = "user2", TopicName = "topic-b" });

            // Act - Publish message to topic-a
            var message = new TopicMessage
            {
                TopicName = "topic-a",
                Content = "Test message for topic A",
                SenderId = "test-sender"
            };
            await topicManager.Ask(new PublishToTopic { Message = message });

            // Allow time for message processing
            await Task.Delay(100);

            // Assert - Check messages for both users
            var user1Messages = await user1Actor.Ask<object>(new CheckForMessages { UserId = "user1" });
            var user2Messages = await user2Actor.Ask<object>(new CheckForMessages { UserId = "user2" });

            // User1 should receive the message
            Assert.IsType<TopicMessage>(user1Messages);
            var user1Message = (TopicMessage)user1Messages;
            Assert.Equal("Test message for topic A", user1Message.Content);
            Assert.Equal("topic-a", user1Message.TopicName);

            // User2 should receive a heartbeat (no message)
            Assert.IsType<HeartbeatNotification>(user2Messages);
        }

        [Fact]
        public async Task User3_JoinsLate_CanReceiveHistoricalMessages_UpTo100()
        {
            // Arrange
            var topicManager = Sys.ActorOf(Props.Create<TopicManagerActor>(), "topic-manager-2");
            
            // Register user3 and subscribe to topic-c
            var user3Actor = await topicManager.Ask<IActorRef>(new RegisterUser { UserId = "user3" });
            await topicManager.Ask(new SubscribeToTopic { UserId = "user3", TopicName = "topic-c" });

            // Act - Publish multiple messages before user3 connects
            var messages = new List<TopicMessage>();
            for (int i = 0; i < 10; i++)
            {
                var msg = new TopicMessage
                {
                    TopicName = "topic-c",
                    Content = $"Historical message {i}",
                    SenderId = "historical-sender"
                };
                messages.Add(msg);
                await topicManager.Ask(new PublishToTopic { Message = msg });
            }

            // Allow time for message processing
            await Task.Delay(200);

            // Assert - Get user3's history
            var historyResponse = await user3Actor.Ask<UserHistoryResponse>(new GetUserHistory { UserId = "user3" });
            
            Assert.NotNull(historyResponse);
            Assert.Equal(10, historyResponse.Messages.Count);
            
            // Verify messages are in order
            for (int i = 0; i < 10; i++)
            {
                Assert.Equal($"Historical message {i}", historyResponse.Messages[i].Content);
            }
        }

        [Fact]
        public async Task MessageHistory_LimitsTo100Messages()
        {
            // Arrange
            var topicManager = Sys.ActorOf(Props.Create<TopicManagerActor>(), "topic-manager-3");
            
            // Register user4 and subscribe to topic-d
            var user4Actor = await topicManager.Ask<IActorRef>(new RegisterUser { UserId = "user4" });
            await topicManager.Ask(new SubscribeToTopic { UserId = "user4", TopicName = "topic-d" });

            // Act - Publish 150 messages
            for (int i = 0; i < 150; i++)
            {
                var msg = new TopicMessage
                {
                    TopicName = "topic-d",
                    Content = $"Message {i}",
                    SenderId = "bulk-sender"
                };
                await topicManager.Ask(new PublishToTopic { Message = msg });
            }

            // Allow time for message processing
            await Task.Delay(500);

            // Assert - Get user4's history
            var historyResponse = await user4Actor.Ask<UserHistoryResponse>(new GetUserHistory { UserId = "user4" });
            
            Assert.NotNull(historyResponse);
            Assert.Equal(100, historyResponse.Messages.Count);
            
            // Verify we have the latest 100 messages (50-149)
            Assert.Equal("Message 50", historyResponse.Messages[0].Content);
            Assert.Equal("Message 149", historyResponse.Messages[99].Content);
        }

        [Fact]
        public async Task MultipleUsers_SubscribeToSameTopic_AllReceiveMessages()
        {
            // Arrange
            var topicManager = Sys.ActorOf(Props.Create<TopicManagerActor>(), "topic-manager-4");
            
            // Register multiple users
            var user5Actor = await topicManager.Ask<IActorRef>(new RegisterUser { UserId = "user5" });
            var user6Actor = await topicManager.Ask<IActorRef>(new RegisterUser { UserId = "user6" });
            var user7Actor = await topicManager.Ask<IActorRef>(new RegisterUser { UserId = "user7" });

            // All subscribe to same topic
            await topicManager.Ask(new SubscribeToTopic { UserId = "user5", TopicName = "topic-shared" });
            await topicManager.Ask(new SubscribeToTopic { UserId = "user6", TopicName = "topic-shared" });
            await topicManager.Ask(new SubscribeToTopic { UserId = "user7", TopicName = "topic-shared" });

            // Act - Publish message to shared topic
            var message = new TopicMessage
            {
                TopicName = "topic-shared",
                Content = "Broadcast message",
                SenderId = "broadcaster"
            };
            var publishResult = await topicManager.Ask(new PublishToTopic { Message = message });

            // Allow time for message processing
            await Task.Delay(100);

            // Assert - All users should receive the message
            var user5Message = await user5Actor.Ask<object>(new CheckForMessages { UserId = "user5" });
            var user6Message = await user6Actor.Ask<object>(new CheckForMessages { UserId = "user6" });
            var user7Message = await user7Actor.Ask<object>(new CheckForMessages { UserId = "user7" });

            Assert.IsType<TopicMessage>(user5Message);
            Assert.IsType<TopicMessage>(user6Message);
            Assert.IsType<TopicMessage>(user7Message);

            Assert.Equal("Broadcast message", ((TopicMessage)user5Message).Content);
            Assert.Equal("Broadcast message", ((TopicMessage)user6Message).Content);
            Assert.Equal("Broadcast message", ((TopicMessage)user7Message).Content);
        }
    }
}