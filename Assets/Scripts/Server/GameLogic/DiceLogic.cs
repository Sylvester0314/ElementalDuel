using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Enums;
using Unity.Netcode;

namespace Server.GameLogic
{
    public class DiceLogic : INetworkSerializable, IEquatable<DiceLogic>, IComparable<DiceLogic>
    {
        public ResourceLogic Logic;
        
        public string Id;
        public CostType Type;
        public int Weight;
        public bool Choosing;

        public static List<DiceLogic> EmptyList = Array.Empty<DiceLogic>().ToList();
        
        public DiceLogic() { }
        
        public DiceLogic(ResourceLogic logic, CostType type)
        {
            Logic = logic;
            
            Id = Guid.NewGuid().ToString();
            Type = type;
            CalculateWeight();
        }

        public void CalculateWeight()
        {
            Weight = CostType.Any - Type;

            if (Logic.AliveElements.Contains(Type))
                Weight += 20;
            if (CostType.Any == Type)
                Weight += 100;
        }

        public bool Match(CostType type)
        {
            return Type == type || Type == CostType.Any;
        }

        public void ModifyType(CostType type)
        {
            Type = type;
            CalculateWeight();
        }

        public void Tuning(CostType type)
        {
            ModifyType(type);
            Logic.PlayerLogic.Game.TurnManager.DoAction(false);
        }

        public DiceLogic Clone(ResourceLogic logic)
        {
            return new DiceLogic(logic, Type) { Id = Id };
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Weight);
        }
    
        public bool Equals(DiceLogic other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Id == other.Id;
        }
        
        public int CompareTo(DiceLogic other)
        {
            return other.Weight.CompareTo(Weight);
        }
    }
}