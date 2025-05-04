using System.Collections.Generic;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Misc;

namespace Server.Logic.Event
{
    public class HealEvent : AttributeModifiableEvent
    {
        public HealEvent(IEventSource source, IEventTarget target, IEventGenerator via, int amount)
            : base(source, target, via)
        {
            Amount = amount;
        }

        public override List<BaseEvent> Execute(ResolveTree resolve)
        {
            if (Target is not CharacterData character)
                return EmptyList;
            
            if (!character.Defeated)
                character.ModifyHealth(ref Amount, 1);
            
            resolve.Events.Add(this);
            
            return new List<BaseEvent> { this };
        }

        public override void WriteToOverview(ResolveOverview overview)
        {
            var modification = overview.Modifications[Target.UniqueId];

            modification.Modified = true;
            modification.HealthModified += Amount;
            modification.HealReceived = true;
        }

        public override void Log() => Logger
            .Append("Amount: ").AppendLine(Amount.ToString())
            .Print();
    }
}