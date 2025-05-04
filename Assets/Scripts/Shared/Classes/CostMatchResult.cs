using System;
using Server.GameLogic;
using Shared.Handler;
using Unity.Netcode;

namespace Shared.Classes
{
    public class CostMatchResult : INetworkSerializable, IEquatable<CostMatchResult>
    {
        public bool Usable;
        public CostLogic Cost;
        public ResourceMatchedResult MatchedResult;

        public CostMatchResult()
        {
            Cost = new CostLogic();
            MatchedResult = new ResourceMatchedResult();
        }

        public CostMatchResult(PlayerLogic logic, ICostHandler entity)
        {
            Cost = entity.Cost;
            Cost.ResetActualCost();
            
            entity.CalculateActualCost();
            
            MatchedResult = logic.Resource.Match(Cost);
            Usable = MatchedResult.Success && entity.EvaluateUsable();
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Usable);
            serializer.SerializeValue(ref Cost);
            serializer.SerializeValue(ref MatchedResult);
        }

        public bool Equals(CostMatchResult other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return Usable == other.Usable &&
                   Cost.Equals(other.Cost) &&
                   MatchedResult.Equals(other.MatchedResult);
        }
    }
}