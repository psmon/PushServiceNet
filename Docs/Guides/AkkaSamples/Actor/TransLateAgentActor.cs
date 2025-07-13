using ActorLib.Modules;
using ActorLib.Modules.TransLate;
using Akka.Actor;
using Akka.Event;
using GouServiceAPI.Repositories;

namespace GouServiceAPI.Actor
{
    public class TransLateAgentCommad { }

    public class LangTransQueueUpdateCommand : TransLateAgentCommad
    {
        public string AgentName { get; set; }
    }

    /// <summary>
    /// 번역큐에서 번역을 처리하는 Agent
    /// </summary>

    public class TransLateAgentActor : ReceiveActor, IWithTimers
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        private readonly IServiceProvider _serviceProvider;

        private int ErrorCount = 100;

        private string _agentName;

        private sealed class TimerKey
        {
            public static readonly TimerKey Instance = new();
            private TimerKey() { }
        }

        public ITimerScheduler Timers { get; set; } = null!;


        /// <summary>
        /// TransLateAgentActor 클래스의 인스턴스를 초기화합니다.
        /// </summary>
        public TransLateAgentActor(IServiceProvider serviceProvider, string agentName, int refreshTimeSec)
        {

            logger.Info($"TransLateAgentActor : Constructor - {Self.Path}");

            _serviceProvider = serviceProvider;

            _agentName = agentName;

            // refreshTimeSec 초마다 번역큐를 조회하여 번역을 처리합니다.
            Timers.StartPeriodicTimer(
                key: TimerKey.Instance,
                msg: new LangTransQueueUpdateCommand()
                {
                    AgentName = agentName
                },
                initialDelay: TimeSpan.FromSeconds(3),
                interval: TimeSpan.FromSeconds(refreshTimeSec));


            ReceiveAsync<LangTransQueueUpdateCommand>( async msg =>
            {
                if(ErrorCount > 100)
                {
                    logger.Error("### Total ErrorCount over(100), Please check your system");
                    return;
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var memberRepository = scope.ServiceProvider.GetRequiredService<LangTransQueueRepository>();
                    
                    var translate = scope.ServiceProvider.GetRequiredService<ITranslateEngine>();

                    var result_step1 = await memberRepository.UpdateQueueAsync(msg.AgentName);

                    if (result_step1 == null)
                    {
                        //logger.Info("### No Data");
                        return;
                    }

                    long langTransNo = result_step1.LangTransNo;
                    string contentsKey = result_step1.ContentsKey.ToString();
                    string contents_Original = result_step1.Contents_Original;
                    string languageCode = result_step1.LanguageCode;

                    string statusCode = "02";
                    string resultCode = "0000";
                    string resultMessage = "SUCCESS";
                    string translatedContent = string.Empty;

                    try
                    {
                        logger.Info($"### Try TranslationAsync - {msg.AgentName},{contentsKey}");

                        var translateInfo = await translate.TranslationAsync(contents_Original, languageCode);

                        translatedContent = translateInfo[0].translations[0].text;                        
                    }
                    catch(Exception e)
                    {
                        logger.Error(e, $"Failed to TranslationAsync - {contentsKey}");
                        statusCode = "00";
                        resultMessage = "Failed-Translation";
                        resultCode = "0001";
                        ErrorCount++;                        
                    }
                    finally
                    {                                                
                        logger.Info($"### Try CompleteQueueAsync - {msg.AgentName},{contentsKey},{statusCode}");
                        var result_step2 = await memberRepository.CompleteQueueAsync(langTransNo, contentsKey, translatedContent, languageCode,
                            msg.AgentName, statusCode, resultCode, resultMessage);
                    }
                }
            });
        }
    }
}
