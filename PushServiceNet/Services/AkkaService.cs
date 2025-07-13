using Akka.Actor;
using Akka.Hosting;

namespace PushServiceNet.Services
{
    public class AkkaService
    {
        private readonly ActorSystem _actorSystem;
        private readonly Dictionary<string, IActorRef> _actors = new Dictionary<string, IActorRef>();

        public AkkaService(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        public ActorSystem GetActorSystem()
        {
            return _actorSystem;
        }

        public void AddActor(string name, IActorRef actor)
        {
            if (!_actors.ContainsKey(name))
            {
                _actors[name] = actor;
            }
        }

        public IActorRef? GetActor(string name)
        {
            return _actors.TryGetValue(name, out var actor) ? actor : null;
        }

        public IActorRef GetOrCreateActor<T>(string name, params object[] args) where T : ActorBase
        {
            var actor = GetActor(name);
            if (actor == null)
            {
                var props = Props.Create<T>(args);
                actor = _actorSystem.ActorOf(props, name);
                AddActor(name, actor);
            }
            return actor;
        }
    }
}