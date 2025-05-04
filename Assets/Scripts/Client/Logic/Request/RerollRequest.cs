using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Logic.Response;
using Server.Managers;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Request
{
    public class RerollRequest : BaseRequest, IEquatable<RerollRequest>
    {
        public List<string> SelectingIds;
        
        public RerollRequest()
        {
            SelectingIds = StaticMisc.EmptyStringList;
        }

        public RerollRequest(List<string> selecting)
        {
            SelectingIds = selecting;
        }
        
        public static void HalfComplete(NetworkRoom room, ulong requesterId)
        {
            var response = new RerollResponse(requesterId, times: -2);
            var wrappers = ActionResponseWrapper.Package(response);
            var rpcParam = NetCodeMisc.RpcParamsWrapper(requesterId);
                
            room.ResponseClientRpc(wrappers, null, rpcParam);
        }
    
        public static void BothComplete(NetworkRoom room, ulong requesterId)
        {
            var response = new RerollResponse(requesterId, times: -3);
            var wrappers = ActionResponseWrapper.Package(response);

            room.ResponseClientRpc(wrappers);
            room.GameManager.StartActionPhase();
        }

        public override async Task Process()
        {
            var dices = Logic.Resource.Reroll(SelectingIds);
            var response = new RerollResponse(RequesterId, dices);
            var wrappers = ActionResponseWrapper.Package(response);
            
            Response(wrappers);
            Game.Receiver.Dequeue(UniqueId);
            
            await Task.CompletedTask;
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            NetCodeMisc.SerializeList(serializer, ref SelectingIds);
        }

        public bool Equals(RerollRequest other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return SelectingIds.Equals(other.SelectingIds);
        }

        public override string ToString()
        {
            return $"RerollRequest(Amount: {SelectingIds.Count})";
        }
    }
}