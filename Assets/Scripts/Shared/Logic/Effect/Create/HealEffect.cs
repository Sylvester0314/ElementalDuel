using Server.Logic.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Shared.Handler;
using Shared.Logic.Statuses;

namespace Shared.Logic.Effect
{
    [Serializable]
    public class HealEffect : AttributeModifiableEffect
    {
        public int healAmount;

        protected override IReadOnlyList<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via)
        {
            var character = (via as SkillLogic)?.Owner;
            var targets = GetTargets(source.Belongs, character);
            
            return targets.Select(data => new HealEvent(source, data, via, healAmount)).ToList();
        }

        public override IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e)
        {
            var events = GenerateEvents(handler, e.Via);

            if (events.Count != 0)
                return events.Concat(AutoConsume(handler)).ToList();

            return events;
        }
    }
}