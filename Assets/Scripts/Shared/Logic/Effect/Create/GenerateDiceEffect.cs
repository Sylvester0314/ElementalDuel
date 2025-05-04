using Server.Logic.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Statuses;
using Shared.Misc;
using UnityEngine;

namespace Shared.Logic.Effect
{
    public enum ResourceType
    {
        Cryo,
        Hydro,
        Pyro,
        Electro,
        Geo,
        Dendro,
        Anemo,
        Omni,
        Basic,
        Adaptive
    }
    
    [Serializable]
    public class GenerateDiceEffect : BaseNonTargetCreateEffect
    {
        public ResourceType type;
        [Range(1, 16)]
        public int amount = 1;
        
        protected readonly List<CostType> BasicDiceTypes = new ()
        {
            CostType.Cryo,      CostType.Hydro,
            CostType.Pyro,      CostType.Anemo,
            CostType.Electro,   CostType.Dendro,   CostType.Geo
        };
        
        protected override IReadOnlyList<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via)
        {
            var events = new List<BaseEvent>();

            if (site != Site.Self)
                events.Add(Generate(source.Opponent, source, via));
            if (site != Site.Opponent)
                events.Add(Generate(source.Belongs, source, via));

            return events;
        }

        public override IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e)
            => GenerateEvents(handler, e.Via).Concat(AutoConsume(handler)).ToList();
        
        private BaseEvent Generate(PlayerLogic player, IEventSource source, IEventGenerator via)
        {
            var result = new List<DiceLogic>();
            var active = player.ActiveCharacter;
            var resource = player.Resource;

            var costType = type switch
            {
                ResourceType.Adaptive => GetAdaptiveType(active),
                ResourceType.Omni     => CostType.Any,
                _                     => (CostType)type
            };

            if (type is not ResourceType.Basic)
            {
                for (var i = 0; i < amount; i++)
                    result.Add(new DiceLogic(resource, costType));
            }
            else
            {
                var random = player.Game.Random;
                BasicDiceTypes.Shuffle(random);
                
                var firstTake = Math.Min(amount, BasicDiceTypes.Count);
                var takenDice = BasicDiceTypes
                    .Take(firstTake)
                    .Select(cost => new DiceLogic(resource, cost))
                    .ToList();
                result.AddRange(takenDice);

                var secondTake = amount - firstTake;
                for (var i = 0; i < secondTake; i++)
                {
                    costType = BasicDiceTypes[random.NextInt(7)];
                    result.Add(new DiceLogic(resource, costType));
                }
            }

            return new GenerateDiceEvent(source, player.ActiveCharacter, via, result);
        }
        
        private CostType GetAdaptiveType(IEventSource source)
            => source switch
            {
                CharacterData character                    => character.Element,
                Status { Type : StatusType.Status } status => status.Parent.Belongs.Element,
                _                                          => source.Belongs.ActiveCharacter.Element
            };
    }
}