using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Logic.Response;
using Server.Managers;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Request
{
    public class SwitchCardRequest : BaseRequest, IEquatable<SwitchCardRequest>
    {
        public bool IsStarting;
        public List<Int> Returns;

        public SwitchCardRequest()
        { 
            Returns = Int.EmptyList;
        }

        public SwitchCardRequest(List<DeckCard> cards, bool starting)
        {
            Returns = cards.Select(card => card.timestamp).Packing<int, Int>();
            IsStarting = starting;
        }
        
        public static void HalfComplete(NetworkRoom room, ulong id)
        {
            var headerResponse = PromptResponse.Header("header_starting_hand");
            var signalResponse = PromptResponse.Signal("waiting_oppo_switch", true);

            var responses = new IActionResponse[] { headerResponse, signalResponse };
            var wrappers = ActionResponseWrapper.Package(responses);
            var rpcParam = NetCodeMisc.RpcParamsWrapper(id);
            
            room.ResponseClientRpc(wrappers, null, rpcParam);
        }
    
        public static void BothComplete(NetworkRoom room, ulong id)
        {
            var opponent = room.GameManager.RequesterLogic(id).Opponent;
            
            var responses = new IActionResponse[]
            {
                PromptResponse.Close(true), 
                SwitchCardResponse.Back(id),
                SwitchCardResponse.Back(opponent.Id)
            };
            var wrappers = ActionResponseWrapper.Package(responses);
            
            room.ResponseClientRpc(wrappers);
        }

        public override async Task Process()
        {
            var switchedList = Logic.DeckCard.Switch(Returns.Unpacking());
            var response = new SwitchCardResponse(RequesterId, SwitchSignal.Result, switchedList, IsStarting);
            var wrappers = ActionResponseWrapper.Package(response);

            Response(wrappers);
            
            await base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref IsStarting);
            NetCodeMisc.SerializeList(serializer, ref Returns);
        }

        public bool Equals(SwitchCardRequest other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Returns.SequenceEqual(other.Returns);
        }

        public override string ToString()
        {
            return $"SwitchCard(IsStarting={IsStarting}, Amount={Returns.Count}, Returns={Returns.ToLog()})";
        }
    }
}