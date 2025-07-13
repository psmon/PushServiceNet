using Akka.Actor;

namespace GouServiceAPI.Actor
{
    public class AgentManagerActorCommand {}

    public class CreateAgentCommand : AgentManagerActorCommand
    {        
        public string AgentName { get; set; }

        public int RefreshTimeSec { get; set; }
    }

    public class DeleteAgentCommand : AgentManagerActorCommand
    {
        public string AgentName { get; set; }
    }

    public class AgentManagerActor : ReceiveActor
    {
        private readonly IServiceProvider _serviceProvider;

        public AgentManagerActor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Receive<CreateAgentCommand>(msg =>
            {
                var agent = Context.ActorOf(Props.Create(() => new TransLateAgentActor(_serviceProvider, msg.AgentName, 
                    msg.RefreshTimeSec)), msg.AgentName);
            });

            Receive<DeleteAgentCommand>(msg =>
            {
                var agent = Context.Child(msg.AgentName);
                if (agent != ActorRefs.Nobody)
                {
                    Context.Stop(agent);
                }
            });
        }
    }
}
