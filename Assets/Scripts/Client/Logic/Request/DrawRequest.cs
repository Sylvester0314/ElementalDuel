using System;
using System.Threading.Tasks;
using Client.Logic.Response;
using Unity.Netcode;

namespace Client.Logic.Request
{
    public class DrawRequest : BaseRequest, IEquatable<DrawRequest>
    {
        public int Amount;
        
        public DrawRequest() { }

        public DrawRequest(int amount)
        {
            Amount = amount;
        }

        public override async Task Process()
        {
            var (drewList, overdrewList) = Logic.DeckCard.Draw(Amount);
            var response = new DrawResponse(RequesterId, drewList, overdrewList);
            var wrappers = ActionResponseWrapper.Package(response);
            
            Response(wrappers);
            
            await base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref Amount);
        }

        public bool Equals(DrawRequest other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Amount == other.Amount;
        }

        public override string ToString()
        {
            return $"Draw(Amount: {Amount})";
        }
    }
}