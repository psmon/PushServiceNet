using Akka.Actor;

using Akka.Actor;
using Akka.Event;

using GouServiceAPI.Repositories;

using NLog;

namespace GouServiceAPI.Actor.Test
{
    /// <summary>
    /// 데이터베이스 접근을 테스트하기 위한 액터 클래스입니다.
    /// </summary>
    public class TestDbAccessActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        private readonly IServiceProvider _serviceProvider;

        private IActorRef? testProbe;

        /// <summary>
        /// TestDbAccessActor 클래스의 인스턴스를 초기화합니다.
        /// </summary>
        public TestDbAccessActor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            logger.Info("TestDbAccessActor : Constructor");

            // IActorRef 메시지를 수신합니다.
            Receive<IActorRef>(actorRef =>
            {
                testProbe = actorRef;

                testProbe.Tell("done");
            });

            // 문자열 메시지를 수신합니다.
            Receive<string>(msg =>
            {
                logger.Info($"Received message : {msg}");

                if (msg == "Test")
                {
                    logger.Info("TestDbAccessActor : Test");

                    testProbe.Tell("Test");
                }

                if (msg == "TestDB")
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var memberRepository = scope.ServiceProvider.GetRequiredService<MemberRepository>();
                        
                        dynamic member = memberRepository.LogInAsync("test2", "1234", "EM").Result;
                        logger.Info($"Member: {member}");
                        testProbe.Tell("TestDB");
                    }
                }
            });
        }

    }
}
