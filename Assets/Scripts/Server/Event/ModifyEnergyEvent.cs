using System;
using System.Collections.Generic;
using Client.Logic.Response;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Misc;

namespace Server.Logic.Event
{
    public class ModifyEnergyEvent : PreviewableEvent
    {
        public int Amount;
        
        public ModifyEnergyEvent(IEventSource source, IEventTarget target, IEventGenerator via, int amount)
            : base(source, target, via)
        {
            Amount = amount;
            EventId = Guid.NewGuid();
        }
        
        public static ModifyEnergyEvent Create(CharacterData character, IEventGenerator via, int amount)
            => new (character, character, via, amount);

        public override List<BaseEvent> Execute(ResolveTree resolve)
        {
            if (Target is CharacterData character)
            {
                character.ModifyEnergy(ref Amount);
                resolve.Events.Add(this);
            }
            
            return EmptyList;
        }

        public override void WriteToOverview(ResolveOverview overview)
        {
            var modification = overview.Modifications[Target.UniqueId];

            modification.Modified = true;
            modification.EnergyModified += Amount;
        }

        public override IReadOnlyList<IActionResponse> ToResponses()
            => new ModifyEnergyResponse(Amount, Target.UniqueId).SingleList();

        public override void Log() => Logger
            .Append("Amount: ").AppendLine(Amount.ToString())
            .Print();
    }
}