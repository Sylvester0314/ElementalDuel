using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Shared.Misc
{
    public static class NetCodeMisc
    {
        public static void WriteValueSafe(this FastBufferWriter writer, in List<string> list)
        {
            writer.WriteValueSafe(list.Count);
            foreach (var item in list)
                writer.WriteValueSafe(item);
        }
        
        public static void ReadValueSafe(this FastBufferReader reader, out List<string> list)
        {
            reader.ReadValueSafe(out int count);
            list = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out string item);
                list.Add(item);
            }
        }

        public static ClientRpcParams RpcParamsWrapper(ulong playerId)
        {
            var send = new ClientRpcSendParams { TargetClientIds = new[] { playerId } };
            return new ClientRpcParams { Send = send };
        }

        public static ClientRpcParams RpcParamsWrapper(List<ulong> playerIds)
        {
            var send = new ClientRpcSendParams { TargetClientIds = playerIds };
            return new ClientRpcParams { Send = send };
        }

        public static void SerializeList<T, TU>
            (BufferSerializer<T> serializer, ref List<TU> list)
            where T : IReaderWriter
            where TU : INetworkSerializable, new()
        {
            var length = list?.Count ?? 0;
            serializer.SerializeValue(ref length);

            if (serializer.IsWriter && list != null)
                for (var i = 0; i < length; i++)
                {
                    var temp = list[i];
                    serializer.SerializeValue(ref temp);
                }
            
            else if (serializer.IsReader)
            {
                list = new List<TU>(length);
                var temp = GetDefault<TU>();
                for (var i = 0; i < length; i++, temp = GetDefault<TU>())
                { 
                    serializer.SerializeValue(ref temp);
                    list.Add(temp);
                }
            }
        }
        
        public static void SerializeList<T>(BufferSerializer<T> serializer, ref List<string> list) where T : IReaderWriter
        {
            var length = list?.Count ?? 0;
            serializer.SerializeValue(ref length);

            if (serializer.IsWriter && list != null)
                for (var i = 0; i < length; i++)
                {
                    var temp = list[i];
                    serializer.SerializeValue(ref temp);
                }
            
            else if (serializer.IsReader)
            {
                list = new List<string>(length);
                var temp = "";
                for (var i = 0; i < length; i++, temp = "")
                { 
                    serializer.SerializeValue(ref temp);
                    list.Add(temp);
                }
            }
        }
        
        public static void SerializeDictionary<T, TV>
            (BufferSerializer<T> serializer, ref Dictionary<string, TV> dict)
            where T : IReaderWriter
            where TV : INetworkSerializable, new()
        {
            var keys = dict?.Keys.ToList() ?? new List<string>();
            var values = dict?.Values.ToList() ?? new List<TV>();
            
            SerializeList(serializer, ref keys);
            SerializeList(serializer, ref values);
            
            if (!serializer.IsReader)
                return;
            
            dict = new Dictionary<string, TV>(keys.Count);
            for (var i = 0; i < keys.Count; i++)
                dict.Add(keys[i], values[i]);
        }
        
        public static List<TR> Packing<T, TR>(this IEnumerable<T> list) where TR : NetworkType<T>, new ()
        {
            return list.Select(content => new TR { content = content }).ToList();
        }
        
        public static List<T> Unpacking<T>(this IReadOnlyList<NetworkType<T>> list)
        {
            return list.Select(element => element.content).ToList();
        }
        
        public static T GetDefault<T>() where T : new()
        {
            return new T();
        }
    }

    [Serializable]
    public class PlayerInformation : INetworkSerializable, IEquatable<PlayerInformation>
    {
        public ulong clientId;
        public string nakamaId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref nakamaId);
        }

        public bool Equals(PlayerInformation other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return clientId == other.clientId && nakamaId == other.nakamaId;
        }
    }
}