using System;
using Client.Logic.Request;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class ReceiveCallbackResponse : BaseResponse, IEquatable<ReceiveCallbackResponse>
    {
        public string UniqueId;
        
        public ReceiveCallbackResponse() { }

        public ReceiveCallbackResponse(ulong id, string uniqueId, Global global) : base(id)
        {
            UniqueId = uniqueId;
            Global = global;
        }

        public override void Process()
        {
            if (NextResponse == null)
                Global.CurrentResponse = null;

            var request = new ReceiveCallbackRequest(UniqueId);
            var wrapper = ActionRequestWrapper.CreatUnblock(request);
            Request(wrapper);
            
            base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref UniqueId);
        }

        public bool Equals(ReceiveCallbackResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return UniqueId == other.UniqueId;
        }
    }
}