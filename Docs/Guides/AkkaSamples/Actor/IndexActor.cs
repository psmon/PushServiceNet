using Akka.Actor;

using Akka.Event;

using GouServiceAPI.Repositories.Elk;

namespace GouServiceAPI.Actor
{
    public class IndexActorCommad { }

    public class ContentIndexUpdateCommand : IndexActorCommad
    {        
    }

    public class IndexActor : ReceiveActor, IWithTimers
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        private readonly IServiceProvider _serviceProvider;

        private bool isInitSync = false;

        private DateTime lastSyncTime = DateTime.UtcNow.AddYears(-100);

        public int RefreshTimeSecForContentIndexUpdate { get; set; } = 30;


        private sealed class TimerKey
        {
            public static readonly TimerKey Instance = new();
            private TimerKey() { }
        }

        public ITimerScheduler Timers { get; set; } = null!;

        public IndexActor(IServiceProvider serviceProvider) 
        {
            logger.Info($"TransLateAgentActor : Constructor - {Self.Path}");

            // refreshTimeSec 초마다 번역큐를 조회하여 번역을 처리합니다.
            Timers.StartPeriodicTimer(
                key: TimerKey.Instance,
                msg: new ContentIndexUpdateCommand(),                
                initialDelay: TimeSpan.FromSeconds(10),
                interval: TimeSpan.FromSeconds(RefreshTimeSecForContentIndexUpdate));

            _serviceProvider = serviceProvider;

            ReceiveAsync<ContentIndexUpdateCommand>( async msg =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var indexRepository = scope.ServiceProvider.GetRequiredService<IndexRepository>();

                    if (isInitSync)
                    {                        
                        //TODO : 업데이트본만 갱신하기 ( 현재 : 하루전꺼 지속갱신)
                        var docCount = await indexRepository.IncrementndexDokumentAsync(lastSyncTime.AddDays(-1));
                        if (docCount > 0)
                        {
                            lastSyncTime = DateTime.Now;
                            logger.Info($"IndexActor : IncrementIndexDokumentAsync - {docCount} - {lastSyncTime}");                            
                        }
                    }
                    else
                    {
                        // 전체 인덱싱 수행
                        isInitSync = true;
                        lastSyncTime = DateTime.Now;
                        var docCount = await indexRepository.FullBulkIndexDokumentAsync();
                        logger.Info($"IndexActor : FullBulkIndexDokumentAsync - {docCount} - {lastSyncTime}");
                    }
                }
            });
        }


    }
}
