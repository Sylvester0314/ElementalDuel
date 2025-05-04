using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Statuses;
using Shared.Misc;

namespace Server.Logic.Event
{
    public class DamageEvent : AttributeModifiableEvent, IEventModifiable
    {
        public bool IsMainTarget;
        public Element ElementType;
        public DamageType DamageTypes;

        public bool DefeatCharacter;
        public ElementalReaction Reaction = ElementalReaction.None;

        public List<Status> TriggeredStatuses { get; set; }
        public (int increase, int decrease, int amplify, float reduce) Modifiers;

        public DamageEvent(IEventSource source, IEventTarget target, IEventGenerator via)
            : base(source, target, via)
        {
            Modifiers = (0, 0, 0, 1);
            TriggeredStatuses = new List<Status>();
        }

        public override List<BaseEvent> Execute(ResolveTree resolve)
        {
            if (Target is not CharacterData character)
                return EmptyList;

            var reactionEvents = new List<BaseEvent>();
            var catchEvents = new List<BaseEvent>();
            
            if (ElementType is not Element.Piercing)
            {
                catchEvents.AddRange(Trigger.CatchEvents(this, Timing.OnElementalInfuse));

                reactionEvents.AddRange(resolve.Reaction.React(this, character));
                
                catchEvents.AddRange(Trigger.CatchEvents(this, Timing.OnDamageIncrease));
                catchEvents.AddRange(Trigger.CatchEvents(this, Timing.OnDamageAmplify));
                catchEvents.AddRange(Trigger.CatchEvents(this, Timing.OnDamageReduce));
                catchEvents.AddRange(Trigger.CatchEvents(this, Timing.OnDamageDecrease));

                Amount = CalculateDamage();
            }

            resolve.Events.Add(this);
            
            var events = new List<BaseEvent> { this };
            foreach (var e in reactionEvents)
                events.AddRange(e.Execute(resolve));
            
            DefeatCharacter = character.ModifyHealth(ref Amount, -1);
            return events.Concat(catchEvents).ToList();
        }

        public int CalculateDamage()
        {
            var result = Amount;
            var multiplier = Math.Max(Modifiers.amplify, 1) / Modifiers.reduce;

            result += Modifiers.increase;
            result = (int)Math.Ceiling(result * multiplier);
            result = Math.Max(result - Modifiers.decrease, 0);
            
            return result;
        }

        public List<BaseEvent> HandleCharacterDefeated(List<BaseEvent> common, List<BaseEvent> immune, ref bool fail)
        {
            if (Target is not CharacterData character)
                return null;
            
            if (false)
            {
                // TODO 免于被击倒
                // immune.Add(this);
                // return null;
            }
            else
            {
                character.IsAlive = false;
                character.Application = ElementalApplication.None;
                character.CurrentEnergy = 0;
            
                var aliveCount = character.Logic.Characters.Count(data => data.IsAlive);
                fail = aliveCount == 0;
            }
            
            return common;
        }

        public override void WriteToOverview(ResolveOverview overview)
        {
            var modification = overview.Modifications[Target.UniqueId];

            modification.Modified = true;
            modification.DamageTook = true;
            modification.HealthModified -= Amount;

            if (ElementType is not (Element.Physical or Element.None))
            {
                var applied = modification.CurrentApplication;
                var incoming = ElementType.ToApplication();
                
                var reaction = ReactionLogic.GetReaction(applied, incoming, out var remaining);
                if (reaction != ElementalReaction.None)
                    modification.AppendApplications(applied, incoming);

                if (remaining != modification.CurrentApplication)
                    modification.AppliedNewApplication = true;
                
                modification.CurrentApplication = remaining;
            }
            
            if (DefeatCharacter)
                modification.Defeated = true;
        }

        public override void Log() => Logger
                .Append("IsMainTarget: ").AppendLine(IsMainTarget.ToString())
                .Append("Amount: ").AppendLine(Amount.ToString())
                .Append("ElementType: ").AppendLine(ElementType.ToString())
                .Append("DamageTypes: ").AppendLine(DamageTypes.ToString())
                .Print();
    }
}