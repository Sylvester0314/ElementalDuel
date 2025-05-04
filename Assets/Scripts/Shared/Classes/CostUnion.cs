using System;
using Shared.Enums;
using Unity.Netcode;

namespace Shared.Classes
{
    [Serializable]
    public class CostUnion : INetworkSerializable, IEquatable<CostUnion>
    {
        public CostType type;
        public int count;

        public CostUnion() { }
    
        public CostUnion(CostType type, int count)
        {
            this.type = type;
            this.count = count;
        }

        public int Compare(CostUnion cost)
        {
            if (count > cost.count)
                return 1;
            if (count < cost.count)
                return -1;
            return 0;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref type);
            serializer.SerializeValue(ref count);
        }

        public bool Equals(CostUnion other)
        {
            if (ReferenceEquals(other, null))
                return false;
        
            return type == other.type && count == other.count;
        }

        public CostUnion Clone()
        {
            return new CostUnion(type, count);
        }
    }
}