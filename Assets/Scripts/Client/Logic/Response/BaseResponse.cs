using System;
using System.Linq;
using Client.Logic.Request;
using Client.Managers;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public struct ActionResponseWrapper : INetworkSerializable
    {
        public bool Unblock;
        public IActionResponse Response;
        
        public ActionResponseWrapper(IActionResponse response)
        {
            Unblock = false;
            Response = response;
        }

        public static ActionResponseWrapper[] Package(IActionResponse response)
            => new ActionResponseWrapper[] { new(response) };
        
        public static ActionResponseWrapper[] Package(IActionResponse[] responses)
            => responses.Select(Create).ToArray();
        
        public static ActionResponseWrapper[] Union(IActionResponse response, ActionResponseWrapper[] wrappers)
            => Package(response).Concat(wrappers).ToArray();
        
        public static ActionResponseWrapper[] Union(IActionResponse[] responses, ActionResponseWrapper[] wrappers)
            => Package(responses).Concat(wrappers).ToArray();
        
        public static ActionResponseWrapper[] Union(ActionResponseWrapper wrapper, ActionResponseWrapper[] wrappers)
            => wrapper.SingleList().Concat(wrappers).ToArray();

        public static ActionResponseWrapper Create(IActionResponse response)
            => new (response);
        
        public static ActionResponseWrapper CreateUnblock(IActionResponse response)
            => new (response) { Unblock = true };

        public static ActionResponseWrapper[] Resume(ActionResponseWrapper[] wrappers)
            => Union(new ResumeResponse(), wrappers);
        
        public static IActionResponse Chainable(Global global, ActionResponseWrapper[] wrappers, string uniqueId)
        {
            var playerId = NetworkManager.Singleton.LocalClientId;
            var callback = new ReceiveCallbackResponse(playerId, uniqueId, global);
            
            for (var i = 0; i < wrappers.Length; i++)
            {
                var nextResponse = i == wrappers.Length - 1 ? callback : wrappers[i + 1].Response;
                
                wrappers[i].Response.NextResponse = nextResponse;
                wrappers[i].Response.Global = global;
            }
            
            return wrappers[0].Response;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Unblock);
            
            if (serializer.IsReader)
                Reader(serializer);
            else
                Writer(serializer);

            Response.NetworkSerialize(serializer);
        }

        private void Reader<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var typeId = -1;
            serializer.SerializeValue(ref typeId);

            Response = typeId switch
            {
                0 => new ReceiveCallbackResponse(),
                1 => new DrawResponse(),
                2 => new SwitchCardResponse(),
                3 => new SwitchActiveResponse(),
                4 => new TuningResponse(),
                5 => new RerollResponse(),
                6 => new PromptResponse(),
                7 => new UpdateCostsResponse(),
                // 8 => new PreviewPlayCardResponse(),
                9 => new HealthModifiableUnionResponse(),
                10 => new PlayCardResponse(),
                11 => new UseSkillResponse(),
                12 => new ResourceResponse(),
                13 => new StatusResponse(),
                14 => new ModifyEnergyResponse(),
                15 => new ChooseActiveResponse(),
                16 => new GameOverResponse(),
                17 => new ResumeResponse(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void Writer<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var typeId = Response switch
            {
                ReceiveCallbackResponse => 0,
                DrawResponse => 1,
                SwitchCardResponse => 2,
                SwitchActiveResponse => 3,
                TuningResponse => 4,
                RerollResponse => 5,
                PromptResponse => 6,
                UpdateCostsResponse => 7,
                // GenerateStatusResponse => 8,
                HealthModifiableUnionResponse => 9,
                PlayCardResponse => 10,
                UseSkillResponse => 11,
                ResourceResponse => 12,
                StatusResponse => 13,
                ModifyEnergyResponse => 14,
                ChooseActiveResponse => 15,
                GameOverResponse => 16,
                ResumeResponse => 17,
                _ => throw new ArgumentOutOfRangeException()
            };
            serializer.SerializeValue(ref typeId);
        }
    }

    public interface IActionResponse : INetworkSerializable
    {
        public ulong RequesterId { get; set; }
        public Global Global { get; set; }
        public IActionResponse NextResponse { get; set; }
        public IActionResponse Tail { get; }

        public void Process();
    }

    public class BaseResponse : IActionResponse, IEquatable<BaseResponse>
    {
        public ulong RequesterId
        {
            get => Requester;
            set => Requester = value;
        }
        protected ulong Requester;

        public IActionResponse NextResponse { get; set; }
        public Global Global { get; set; }
        protected Player Player => Global.players[IsRequester ? 0 : 1];
        protected PlayerManager Manager => Global.manager;
        protected ulong LocalId => NetworkManager.Singleton.LocalClientId;
        protected bool IsRequester => RequesterId == LocalId;

        public IActionResponse Tail
        {
            get
            {
                IActionResponse response = this;
            
                while(response.NextResponse != null)
                    response = response.NextResponse;

                return response;
            }
        }
        
        public BaseResponse() { }

        public BaseResponse(ulong requester)
        {
            RequesterId = requester;
        }
        
        /**
         * <summary>
         * Use async/await to handle animations on the client side.
         * Once the animation for the current Response is completed,
         * call the base method to process the next Response.<br></br>
         * While running on the client, Responses are processed sequentially
         * as a queue, and the tail of the queue is a callback indicating
         * that all processing for the current Response has been completed.
         * </summary>
         */
        public virtual void Process()
        {
            NextResponse?.Process();
        }

        protected void Request(IActionRequest request)
        {
            Manager.RequestServerRpc(ActionRequestWrapper.Create(request));
        }

        protected void Request(ActionRequestWrapper wrapper)
        {
            Manager.RequestServerRpc(wrapper);
        }

        public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Requester);
        }

        public virtual bool Equals(BaseResponse other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return RequesterId == other.RequesterId;
        }
    }
}