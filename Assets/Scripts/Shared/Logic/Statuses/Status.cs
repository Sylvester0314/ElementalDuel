using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.GameLogic;
using Server.Logic.Event;
using Server.ResolveLogic;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Effect;
using Shared.Misc;
using Unity.Netcode;

namespace Shared.Logic.Statuses
{
    public class Status : IEventSource, INetworkSerializable, IEquatable<Status>
    {
        public string Key;
        public string Name;
        public StatusType Type;
        public StatusLogic Parent;
        
        public bool AutoDiscard;
        public int PerRoundLimitation;
        
        public StatusLifeMode Mode;
        public StatusLifeSetting Usages;
        public StatusLifeSetting Durations;
        
        public EffectVariables Variables;
        public Dictionary<Timing, EffectContainer> RegisteredEffects;

        public string UniqueId { get; set; }
        public string EntityName => Name;
        public PlayerLogic Belongs => Parent.Logic;

        public Status()
        {
            Usages = new StatusLifeSetting();
            Durations = new StatusLifeSetting();
            Variables = new EffectVariables();
            RegisteredEffects = new Dictionary<Timing, EffectContainer>();
        }
        
        public Status(StatusLogic parent, StatusCardAsset asset, GenerateStatusEffect via)
        {
            Parent = parent;

            UniqueId = Guid.NewGuid().ToString();
            Key = asset.name;
            Name = asset.statusName;
            Type = asset.type;
            
            AutoDiscard = asset.autoDiscard;
            PerRoundLimitation = asset.restrictTriggerPerRound
                ? asset.maxTriggersPerRound : int.MaxValue;

            Mode = asset.mode;
            Usages = StatusLifeSetting.Initialize(asset, via, StatusLifeMode.Usages);
            Durations = StatusLifeSetting.Initialize(asset, via, StatusLifeMode.Durations);
            
            Variables = new EffectVariables(asset.variables);
            RegisteredEffects = asset.handlers
                .ToDictionary(
                    pair => pair.timing,
                    pair => new EffectContainer(pair.effects)
                );

            if (Durations != null)
                RegisteredEffects.Add(Timing.OnRoundEnd, EffectContainer.ConsumeDuration);
        }

        public void Regenerate()
        {
            Usages?.TryStack();
            Durations?.TryStack();
        }

        public List<BaseEvent> Discard()
            => new () { StatusConsumeEvent.Discard(this) };
        
        public List<BaseEvent> ConsumeUsage(int count)
        {
            if (Mode.HasFlag(StatusLifeMode.Once))
                return Discard();
            
            if (!Mode.HasFlag(StatusLifeMode.Usages) || Usages == null)
                return BaseEvent.EmptyList;

            if (Usages.Consume(count) && AutoDiscard)
                return Discard();
            
            return new List<BaseEvent> { StatusConsumeEvent.Create(this) };
        }

        public List<BaseEvent> ConsumeDuration()
            => Durations.Consume(1) && AutoDiscard 
                ? Discard() 
                : new List<BaseEvent> { StatusConsumeEvent.Create(this) };

        public List<BaseEvent> Trigger(BaseEvent e, Timing timing)
        {
            if (!RegisteredEffects.TryGetValue(timing, out var effect))
                return BaseEvent.EmptyList;

            return effect.HandleEvents(this, e);
        }
        
        #region Client Tools

        public string GetFieldText(string field)
        {
            if (field.Equals(string.Empty))
                return "";
            
            if (field.Equals("usage") && Usages != null)
                return Usages.remaining.ToString();
            
            if (field.Equals("duration") && Durations != null)
                return Durations.remaining.ToString();
            
            if (!Variables.Data.TryGetValue(field, out var value))
                return string.Empty;
            
            return value.ToString();
        }

        #endregion
        
        #region Misc

        public StringBuilder CreateLogger()
        {
            var builder = new StringBuilder();
            var name = (this as IEventStringify).LocalizedName;

            builder
                .Append(Type.ToString()).Append(":  ").AppendLine(name)
                .Append("Mode: ").AppendLine(Mode.ToString())
                .Append("StatusId: ").AppendLine(UniqueId);

            if (Usages != null)
            {
                builder.Append("Usage(s): ").Append(Usages.remaining)
                       .Append("/").AppendLine(Usages.max.ToString());
            }

            if (Durations != null)
            {
                builder.Append("Duration(s): ").Append(Durations.remaining)
                    .Append("/").AppendLine(Durations.max.ToString());
            }
            
            return builder;
        }

        public void PrintLogger() => CreateLogger().Print();

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Key);
            serializer.SerializeValue(ref Variables);
            
            var uniqueId = UniqueId ?? string.Empty;
            
            if (serializer.IsWriter)
            {
                serializer.SerializeValue(ref uniqueId);
                StatusLifeSetting.SerializeWriter(serializer, ref Usages);
                StatusLifeSetting.SerializeWriter(serializer, ref Durations);
            }

            if (serializer.IsReader)
            {
                serializer.SerializeValue(ref uniqueId);
                UniqueId = uniqueId;
                
                StatusLifeSetting.SerializeReader(serializer, ref Usages);
                StatusLifeSetting.SerializeReader(serializer, ref Durations);
            }
        }

        public bool Equals(Status other)
        {
            if (ReferenceEquals(null, other)) 
                return false;

            return Key.Equals(other.Key);
        }
        
        public Status Clone(StatusLogic logic)
            => new ()
            {
                UniqueId = UniqueId,
                Key = Key,
                Name = Name,
                Type = Type,
                Parent = logic,
                AutoDiscard = AutoDiscard,
                PerRoundLimitation = PerRoundLimitation,
                Mode = Mode,
                Usages = Usages?.Clone(),
                Durations = Durations?.Clone(),
                Variables = Variables.Clone(),
                RegisteredEffects = RegisteredEffects
            };

        #endregion
    }
}