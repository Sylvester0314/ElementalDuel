using System;
using System.Collections.Generic;
using Shared.Misc;
using Unity.Netcode;

namespace Shared.Classes
{
    public enum MatchedResultType
    {
        Empty,
        InsufficientEnergy,
        InsufficientDice,
        InsufficientLegend,
        Successfully
    }

    [Serializable]
    public class ResourceMatchedResult : INetworkSerializable, IEquatable<ResourceMatchedResult>
    {
        public MatchedResultType type;
        
        public int energy;
        public bool legend;
        public List<string> dices;

        public bool Success => type == MatchedResultType.Successfully;
        public string Message => type switch
        {
            MatchedResultType.InsufficientEnergy => "warning_hint_energy",
            MatchedResultType.InsufficientDice   => "warning_hint_dice",
            _                                    => "prerequisite_not_met"
        };

        public ResourceMatchedResult() { }

        public ResourceMatchedResult(MatchedResultType type)
        {
            this.type = type;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref type);
            serializer.SerializeValue(ref energy);
            serializer.SerializeValue(ref legend);
            
            NetCodeMisc.SerializeList(serializer, ref dices);
        }
        
        public bool Equals(ResourceMatchedResult other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return type == other.type && 
                   energy == other.energy &&
                   legend == other.legend &&
                   dices.Equals(other.dices);
        }
    }
}