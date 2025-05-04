using System.Collections.Generic;
using Server.ResolveLogic;
using Shared.Handler;

namespace Server.Logic.Event
{
    public class PassiveEvent : BaseEvent
    {
        public PassiveEvent(IEventSource source, IEventTarget target, IEventGenerator via)
            : base(source, target, via) { }

        public static PassiveEvent Create(IEventSource source, IEventGenerator via = null) 
            => new (source, null, via);

        public override List<BaseEvent> Execute(ResolveTree resolve) 
            => EmptyList;

        public override void Log()
        {
            throw new System.NotImplementedException();
        }
    }
}