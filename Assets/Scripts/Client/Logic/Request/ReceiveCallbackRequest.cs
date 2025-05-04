using System;
using System.Threading.Tasks;

namespace Client.Logic.Request
{
    public class ReceiveCallbackRequest : BaseRequest, IEquatable<ReceiveCallbackRequest>
    {
        public ReceiveCallbackRequest() { }

        public ReceiveCallbackRequest(string uniqueId)
        {
            UniqueId = uniqueId;
        }

        public override async Task Process()
        {
            Game.Receiver.Dequeue(UniqueId);
            await Task.CompletedTask;
        }

        public bool Equals(ReceiveCallbackRequest other)
        {
            return !(ReferenceEquals(other, null) || !base.Equals(other));
        }
    }
}