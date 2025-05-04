using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Shared.Misc
{
    [Serializable]
    public class NetworkType<T> : IEquatable<NetworkType<T>>
    {
        public T content;
        
        public NetworkType() { }

        public NetworkType(T content)
        {
            this.content = content;
        }

        public bool Equals(NetworkType<T> other)
        {
            return other != null && content.Equals(other.content);
        }

        public override string ToString()
        {
            return content.ToString();
        }
    }

    [Serializable]
    public class NetworkPrimitiveType<T> : NetworkType<T>, INetworkSerializable
        where T : unmanaged, IComparable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public NetworkPrimitiveType() { }
        public NetworkPrimitiveType(T content) : base(content) { }
        
        public void NetworkSerialize<TB>(BufferSerializer<TB> serializer) where TB : IReaderWriter
        {
            serializer.SerializeValue(ref content);
        }
    }

    [Serializable]
    public class Int : NetworkPrimitiveType<int>
    {
        public Int() { }
        public Int(int content) : base(content) { }
    
        public static List<Int> EmptyList = Array.Empty<Int>().ToList();
    }
    
    [Serializable]
    public class Ulong : NetworkPrimitiveType<ulong>
    {
        public Ulong() { }
        public Ulong(ulong content) : base(content) { }
    
        public static List<Ulong> EmptyList = Array.Empty<Ulong>().ToList();
    }

    [Serializable]
    public class Bool : NetworkPrimitiveType<bool>
    {
        public Bool() { }
        public Bool(bool content) : base(content) { }
    }

    [Serializable]
    public class String : NetworkType<string>, INetworkSerializable, IEquatable<String>
    {
        public static String Empty = new (string.Empty); 
        public static List<String> EmptyList = Array.Empty<String>().ToList();
        
        public String() { }
        
        public String(string content) : base(content) { }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref content);
        }
    
        public bool Equals(String other)
        {
            return base.Equals(other);
        }
    }
    
    [Serializable]
    public class Enum<T> : NetworkType<T>, INetworkSerializable, IEquatable<Enum<T>> where T : unmanaged, Enum
    {
        public static List<Enum<T>> EmptyList = Array.Empty<Enum<T>>().ToList();

        public Enum() { }
        public Enum(T content) : base(content) { }
        
        public void NetworkSerialize<TB>(BufferSerializer<TB> serializer) where TB : IReaderWriter
        {
            serializer.SerializeValue(ref content);
        }
    
        public bool Equals(Enum<T> other)
        {
            return base.Equals(other);
        }
    }
}