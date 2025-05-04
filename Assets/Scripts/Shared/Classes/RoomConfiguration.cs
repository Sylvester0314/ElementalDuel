using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace Shared.Classes
{
    [Serializable]
    public class RoomConfiguration : INetworkSerializable, IEquatable<RoomConfiguration>
    {
        public string diceMode;
        public string gameMode;
        public string cardPoolPreset;
        public string contemplationTime;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref diceMode);
            serializer.SerializeValue(ref gameMode);
            serializer.SerializeValue(ref cardPoolPreset);
            serializer.SerializeValue(ref contemplationTime);
        }

        public bool Equals(RoomConfiguration other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return diceMode == other.diceMode
                && gameMode == other.gameMode
                && cardPoolPreset == other.cardPoolPreset
                && contemplationTime == other.contemplationTime;
        }
    }

    [Serializable]
    public class RoomInformation
    {
        public string roomId;
        public string ownerUid;
        public int playerCount;
        public List<RoomTagPair> BaseConfigs;
        public List<RoomTagPair> Options;
    }

    public class RoomTagPair
    {
        public string Key;
        public string Entry;

        public RoomTagPair()
        {
        }

        public RoomTagPair(string key, string entry)
        {
            Key = key;
            Entry = entry;
        }
    }
}