using Akka.Actor;

using GouServiceAPI.Models;

namespace GouServiceAPI.Actor
{
    public class SSEUserActor : ReceiveActor
    {
        private Queue<Notification> notifications = new Queue<Notification>();

        private Queue<Notification> notificationHistory = new Queue<Notification>();

        private string IdentyValue { get; set; }

        private IActorRef testProbe;
        public SSEUserActor(string identyValue)
        {
            IdentyValue = identyValue;

            notifications.Enqueue(new Notification()
            {
                Id = IdentyValue,
                NotiType = "System",
                Message = $"[{IdentyValue}] Wellcome... by sse",
                MessageTime = DateTime.Now.ToUniversalTime(),
            });

            ReceiveAsync<IActorRef>(async actorRef =>
            {
                testProbe = actorRef;
                testProbe.Tell("done");
            });

            ReceiveAsync<Notification>(async msg =>
            {
                if (msg.IsConfirm == false)
                {
                    notifications.Enqueue(msg);

                    notificationHistory.Enqueue(msg);

                    if(notificationHistory.Count > 100)
                    {
                        notificationHistory.Dequeue(); // Keep only the last 100 notifications
                    }
                }

            });

            ReceiveAsync<HeartBeatNotification>(async msg =>
            {
                if (testProbe != null)
                {
                    testProbe.Tell(new HeartBeatNotification()
                    {
                        Id = IdentyValue,
                        NotiType = "HeartBeat",
                        MessageTime = DateTime.Now.ToUniversalTime(),
                        
                    });
                }
                else
                {
                    Sender.Tell(new HeartBeatNotification()
                    {
                        Id = IdentyValue,
                        NotiType = "HeartBeat",
                        MessageTime = DateTime.Now.ToUniversalTime(),
                    });
                }
            });

            ReceiveAsync<CheckNotification>(async msg =>
            {
                if (notifications.Count > 0)
                {
                    if (testProbe != null)
                    {
                        testProbe.Tell(notifications.Dequeue());
                    }
                    else
                    {
                        var notification = notifications.Dequeue();

                        // notificationHistory에서 해당 알림의 IsConfirm 값을 true로 변경
                        var historyItem = notificationHistory.FirstOrDefault(n => n.Id == notification.Id && n.Message == notification.Message);
                        if (historyItem != null)
                        {
                            historyItem.IsConfirm = true;
                        }

                        Sender.Tell(notification);
                    }
                }
                else
                {
                    HeartBeatNotification heatBeatNotification = new HeartBeatNotification()
                    {
                        Id = IdentyValue,
                        NotiType = "HeartBeat",
                        MessageTime = DateTime.Now.ToUniversalTime(),
                    };
                    
                    if (testProbe != null)
                    {
                        testProbe.Tell(heatBeatNotification);
                    }
                    else
                    {
                        Sender.Tell(heatBeatNotification);
                    }
                }
            });

            // 알림 히스토리 요청 메시지 처리
            ReceiveAsync<GetNotificationHistory>(async msg =>
            {
                var history = notificationHistory.ToArray(); // Queue를 배열로 변환
                Sender.Tell(new NotificationHistoryResponse
                {
                    History = history
                });
            });

        }
    }
}
