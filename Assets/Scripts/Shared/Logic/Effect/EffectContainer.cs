using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Server.Logic.Event;
using Server.ResolveLogic;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Statuses;
using Shared.Misc;
using Unity.Netcode;
using GeneralEffectLogic = EffectLogic<Shared.Logic.Effect.BaseEffect>;

namespace Shared.Logic.Effect
{
    public class EffectVariables : INetworkSerializable, IEquatable<EffectVariables>
    {
        public Dictionary<string, int> Data;
        
        public static EffectVariables Empty = new();

        public EffectVariables()
        {
            Data = new Dictionary<string, int>();
        }

        public EffectVariables(Dictionary<string, int> variables)
        {
            Data = new Dictionary<string, int>(variables);
        }

        public void Set(string field, int value)
        {
            Data[field] = value;
        }

        public void Modify(string field, int value)
        {
            if (!Data.ContainsKey(field))
                Set(field, 0);
            
            Data[field] += value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var length = Data?.Keys.Count ?? 0;
            var key = string.Empty;
            var value = 0;

            serializer.SerializeValue(ref length);
            
            if (serializer.IsReader)
            {
                Data = new Dictionary<string, int>();

                for (var i = 0; i < length; i++)
                {
                    serializer.SerializeValue(ref key);
                    serializer.SerializeValue(ref value);
                    Data.Add(key, value);
                }
            }

            if (serializer.IsWriter && Data != null)
            {
                foreach (var pair in Data)
                {
                    key = pair.Key;
                    serializer.SerializeValue(ref key);
                    
                    value = pair.Value;
                    serializer.SerializeValue(ref value);
                }   
            }
        }

        public bool Equals(EffectVariables other)
        {
            if (ReferenceEquals(null, other)) 
                return false;

            return Data.Equals(other.Data);
        }
        
        public EffectVariables Clone() => new (Data);
    }

    public class EffectContainer
    {
        public List<GeneralEffectLogic> Effects;

        public EffectContainer(List<GeneralEffectLogic> effects)
        {
            Effects = new List<GeneralEffectLogic>(effects);
        }

        public static EffectContainer ConsumeDuration
            => new(new List<GeneralEffectLogic> { GeneralEffectLogic.ConsumeDurationEffect });

        public List<GeneralEffectLogic> AvailabilityAnalysis(
            BaseEvent e, EffectVariables vars, Status handler = null
        )
        {
            var availabilities = new bool[Effects.Count];

            return Effects.Where((logic, i) =>
            {
                availabilities[i] = logic.condition
                    .Evaluate(e, vars, availabilities, handler);
                return availabilities[i];
            }).ToList();
        }

        public List<BaseEvent> HandleEvents(Status handler, BaseEvent e)
        {
            var effects = AvailabilityAnalysis(e, handler.Variables, handler)
                .SelectMany(logic => logic.subEffects).ToList();
            
            var eventId = Guid.NewGuid();
            var prevEffectType = typeof(BaseEffect);
            var modifiableType = typeof(AttributeModifiableEffect);
            var isModifiable = false;

            return effects.SelectMany(effect =>
            {
                var type = effect.GetType();
                if (type != prevEffectType && (!type.IsSubclassOf(modifiableType) || !isModifiable))
                {
                    prevEffectType = type;
                    eventId = Guid.NewGuid();
                    isModifiable = type.IsSubclassOf(modifiableType);
                }

                return effect
                        .ResponseEvent(handler, e)
                        .Select(@event => @event.UnifiedId(eventId));
            }).ToList();
        }

        public List<BaseCreateEffect> GetAvailableEffects(
            IEventSource source, IEventGenerator via,
            EffectVariables vars, out List<string> branches
        )
        {
            var passive = PassiveEvent.Create(source);
            var logics = AvailabilityAnalysis(passive, vars);
            
            var flattedEffects = logics
                .SelectMany(logic => logic.subEffects.Cast<BaseCreateEffect>().ToList())
                .ToList();
                    
            var selectEffectList = flattedEffects.FindAll(effect => effect.mode == TargetMode.SelectOne);
            if (selectEffectList.Count > 1)
                throw new Exception("A skill definition can only include one valid SelectOne mode");
            
            var damageEffectList = flattedEffects
                .Where(effect => effect is DamageEffect { mainTarget: true }).ToList();
            if (damageEffectList.Count > 1)
                throw new Exception("A skill definition can only have one main target");

            var targetEffectList = flattedEffects
                .Where(effect => effect is not BaseNonTargetCreateEffect &&
                                 effect is not GenerateStatusEffect { target: TargetType.ExtraZone }
                ).ToList();
            
            var character = (via as SkillLogic)?.Owner;
            
            branches = Analysis(selectEffectList)
                ?? Analysis(damageEffectList)
                ?? Analysis(targetEffectList)
                ?? character?.UniqueId?.SingleList()
                ?? new List<string> { ResolveTree.Root };

            return flattedEffects;

            List<string> Analysis(List<BaseCreateEffect> effects)
                => effects
                    .FirstOrDefault()?
                    .GetTargets(via.Belongs, character)?
                    .Select(data => data.UniqueId)
                    .ToList();
        }
    }
}