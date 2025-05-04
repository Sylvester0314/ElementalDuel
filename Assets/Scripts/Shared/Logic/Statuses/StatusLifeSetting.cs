using System;
using Shared.Logic.Effect;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Logic.Statuses
{
    [Flags]
    public enum StatusLifeMode
    {
        None        = 0,
        Once        = 1 << 1,
        Usages      = 1 << 2,
        Durations   = 1 << 3
    }
    
    [Serializable]
    public class StatusLifeSetting : INetworkSerializable, IEquatable<StatusLifeSetting>
    {
        [ShowIf("_overriding")]
        public bool enable;
        
        [HideIf("@_overriding && !enable")]
        public bool stackable;
        [HideIf("@_overriding && !enable")]
        public int initial;
        
        [ShowIf("DisplayStackable")]
        public int max;
        [ShowIf("DisplayStackable")]
        public int repeat;

        [HideInInspector]
        public int remaining;

        private bool _overriding;
        private static StatusLifeSetting _holder = new () { remaining = int.MinValue };

        public StatusLifeSetting() { }
        
        public StatusLifeSetting(bool overriding)
        {
            _overriding = overriding;
            initial = 1;
            max = 2;
            repeat = 0;
        }

        public StatusLifeSetting(StatusLifeSetting setting)
        {
            stackable = setting.stackable;
            initial = setting.initial;
            max = setting.max;
            repeat = setting.repeat;

            remaining = initial;
        }
        
        public static StatusLifeSetting Initialize(
            StatusCardAsset @base, 
            GenerateStatusEffect @override, 
            StatusLifeMode mode
        )
        {
            if (@base.mode == StatusLifeMode.None || !@base.mode.HasFlag(mode))
                return null;
        
            var (baseLife, overrideLife) = mode switch
            {
                StatusLifeMode.Usages    => (@base.usages, @override.usages),
                StatusLifeMode.Durations => (@base.durations, @override.durations),
                _                        => (null, null)
            };
        
            return @base.canOverride && overrideLife.enable
                ? new StatusLifeSetting(overrideLife)
                : new StatusLifeSetting(baseLife);
        }

        public void TryStack()
        {
            var modify = repeat == 0 ? initial : repeat;
            remaining = stackable ? Math.Min(remaining + modify, max) : initial;
        }

        public bool Consume(int count)
        {
            Debug.Log("Before Consume");
            remaining -= count;
            Debug.Log("Consumed: " + remaining);
            return remaining <= 0;
        }

        #region Misc

        public static void SerializeWriter<T>(BufferSerializer<T> serializer, ref StatusLifeSetting setting) 
            where T : IReaderWriter
        {
            if (setting == null)
                serializer.SerializeValue(ref _holder);
            else
                serializer.SerializeValue(ref setting);
        }
        
        public static void SerializeReader<T>(BufferSerializer<T> serializer, ref StatusLifeSetting setting) 
            where T : IReaderWriter
        {
            var value = new StatusLifeSetting();
            serializer.SerializeValue(ref value);

            setting = value.Equals(_holder) ? null : value;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref enable);
            serializer.SerializeValue(ref remaining);
        }

        public bool Equals(StatusLifeSetting other)
        {
            if (ReferenceEquals(null, other)) 
                return false;

            return remaining == other.remaining && 
                   enable  == other.enable &&
                   stackable == other.stackable &&
                   max == other.max &&
                   repeat == other.repeat &&
                   initial == other.initial;
        }
        
        public StatusLifeSetting Clone()
            => new ()
            {
                enable = enable,
                stackable = stackable,
                initial = initial,
                max = max,
                repeat = repeat,
                remaining = remaining
            };
        
        #endregion
        
        private bool DisplayStackable => !_overriding ? stackable : enable && stackable;
    }
}