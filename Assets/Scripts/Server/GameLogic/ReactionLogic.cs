using System;
using System.Collections.Generic;
using System.Linq;
using Server.Logic.Event;
using Shared.Enums;
using Shared.Handler;
using Shared.Misc;

namespace Server.GameLogic
{
    public class ReactionEventGenerator : IEventGenerator
    {
        public string Key => Reaction.ToString();
        public string EntityName => Source.EntityName;
        public PlayerLogic Belongs => Source.Belongs;

        public IEventSource Source;
        public ElementalReaction Reaction;

        public ReactionEventGenerator(IEventSource source, ElementalReaction reaction)
        {
            Source = source;
            Reaction = reaction;
        }
    }
    
    public class ReactionLogic
    {
        public static List<ElementalApplication> Reactions = Enum
            .GetValues(typeof(ElementalReaction))
            .Cast<ElementalReaction>()
            .Skip(1)
            .Select(reaction => (ElementalApplication)reaction)
            .ToList();

        public static ElementalReaction GetReaction(
            ElementalApplication applied, 
            ElementalApplication incoming,
            out ElementalApplication remaining
        )
        {
            var combined = applied | incoming;
            var reaction = Reactions
                .FirstOrDefault(reaction => (combined & reaction) == reaction);
            
            if (reaction is not ElementalApplication.None)
                remaining = combined ^ reaction;
            else if (incoming is not (ElementalApplication.Geo or ElementalApplication.Anemo))
                remaining = combined;
            else
                remaining = applied;
            
            return (ElementalReaction)reaction;
        }

        public IReadOnlyList<BaseEvent> React(DamageEvent damage, CharacterData character)
        {
            if (damage.Reaction != ElementalReaction.None)
                return BaseEvent.EmptyList;
            
            var reaction = GetReaction(
                character.Application,
                damage.ElementType.ToApplication(),
                out var remaining
            );
            var applications = (ElementalApplication)reaction;
            
            damage.Reaction = reaction;
            character.Application = remaining;
            
            if (reaction is ElementalReaction.None)
                return BaseEvent.EmptyList;

            var via = new ReactionEventGenerator(damage.Source, reaction);
            
            if (reaction is ElementalReaction.Melt or ElementalReaction.Burning)
                damage.Modifiers.increase += 2;
            else if (reaction is ElementalReaction.Overloaded)
            {
                damage.Modifiers.increase += 2;

                var target = character.NextCharacter(true).FirstOrDefault();
                if (target != default && character.IsActive)
                    return new SwitchActiveEvent(character, target, via).SingleList();
            }
            else if (reaction is ElementalReaction.Superconduct or ElementalReaction.ElectroCharged)
            {
                damage.Modifiers.increase += 1;
                return ExtraEvents(Element.Piercing, DamageType.None);
            }
            else if (applications.HasFlag(ElementalApplication.Geo))
            {
                damage.Modifiers.increase += 1;
                // TODO 生成结晶
            }
            else if (applications.HasFlag(ElementalApplication.Dendro))
            {
                damage.Modifiers.increase += 1;
                
                var otherApplication = applications ^ ElementalApplication.Dendro;
                // TODO 生成草元素造物
            }
            else if (applications.HasFlag(ElementalApplication.Anemo))
            {
                var otherApplication = applications ^ ElementalApplication.Anemo;
                return ExtraEvents(otherApplication.ToElement(), DamageType.Swirl);
            }
            
            return BaseEvent.EmptyList;

            List<BaseEvent> ExtraEvents(Element damageType, DamageType sourceType)
                => ImpactAdjacentCharacters(damage, character, damageType, sourceType, via);
        }

        // 仅附着
        // public static List<BaseEvent> DoReaction(this CharacterData character, BaseEvent damage)
        // {
            // return new List<BaseEvent>();
        // }

        private List<BaseEvent> ImpactAdjacentCharacters(
            DamageEvent e,
            CharacterData mainTarget,
            Element damageType,
            DamageType sourceType,
            IEventGenerator via
        ) => mainTarget
                .AdjacentCharacters()
                .Select(character => new DamageEvent(e.Source, character, via)
                {
                    EventId = e.EventId,
                    Amount = 1,
                    ElementType = damageType,
                    DamageTypes = sourceType 
                })
                .Cast<BaseEvent>()
                .ToList();
    }
}