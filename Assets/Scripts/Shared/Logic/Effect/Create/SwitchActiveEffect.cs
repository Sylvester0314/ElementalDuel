using Server.Logic.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Handler;
using Shared.Logic.Statuses;

namespace Shared.Logic.Effect
{
    [Serializable]
    public class SwitchActiveEffect : BaseCreateEffect
    {
        protected override IReadOnlyList<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via)
            => GetTargets(source.Belongs)
                .Select(data => new SwitchActiveEvent(data.Belongs.ActiveCharacter, data, via))
                .ToList();
        
        public override IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e)
            => GenerateEvents(handler, e.Via).Concat(AutoConsume(handler)).ToList();
    }
}