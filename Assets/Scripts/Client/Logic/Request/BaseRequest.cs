using System;
using System.Threading.Tasks;
using Client.Logic.Response;
using Server.GameLogic;
using Server.Managers;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Request
{
    public struct ActionRequestWrapper : INetworkSerializable
    {
        public bool Unblock;
        public IActionRequest Request;

        public ActionRequestWrapper(IActionRequest request)
        {
            Unblock = false;
            Request = request;
        }

        public static ActionRequestWrapper Create(IActionRequest request) => new (request);
        
        public static ActionRequestWrapper CreatUnblock(IActionRequest request) 
            => new (request) { Unblock = true };

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Unblock);
            
            if (serializer.IsReader)
                Reader(serializer);
            else
                Writer(serializer);

            Request.NetworkSerialize(serializer);
        }

        private void Reader<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var typeId = 0;
            serializer.SerializeValue(ref typeId);

            Request = typeId switch
            {
                0 => new ReceiveCallbackRequest(),
                1 => new DrawRequest(),
                2 => new SwitchCardRequest(),
                3 => new ChooseActiveRequest(),
                4 => new UseSkillRequest(),
                5 => new PlayCardRequest(),
                6 => new TuningRequest(),
                7 => new RerollRequest(),
                8 => new DeclareEndRequest(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void Writer<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var typeId = Request switch
            {
                ReceiveCallbackRequest => 0,
                DrawRequest => 1,
                SwitchCardRequest => 2,
                ChooseActiveRequest => 3,
                UseSkillRequest => 4,
                PlayCardRequest => 5,
                TuningRequest => 6,
                RerollRequest => 7,
                DeclareEndRequest => 8,
                _ => throw new ArgumentOutOfRangeException()
            };

            serializer.SerializeValue(ref typeId);
        }
    }

    public interface IActionRequest : INetworkSerializable
    {
        public string UniqueId { get; }
        public ulong RequesterId { get; set; }
        public GameManager Game { get; set; }

        public Task Process();
        public Task Process(GameManager manager);
        public void Complete();
    }

    public abstract class BaseRequest : IActionRequest, IEquatable<BaseRequest>
    {
        public ulong RequesterId
        {
            get => Requester;
            set => Requester = value;
        }

        public string UniqueId
        {
            get => Unique;
            set => Unique = value;
        }

        public GameManager Game { get; set; }
        public PlayerLogic Logic => Game.RequesterLogic(RequesterId);
        
        protected ulong Requester;
        protected string Unique;
        protected TaskCompletionSource<bool> Completion;

        protected BaseRequest()
        {
            UniqueId = Guid.NewGuid().ToString();
            RequesterId = NetworkManager.Singleton.LocalClientId;
            Completion = new TaskCompletionSource<bool>();
        }

        // Wait for the client animation to finish playing,
        // and ReceiveCallbackRequest will handle the callback event
        public virtual async Task Process()
        {
            await Completion.Task;
        }

        public void Complete()
        {
            Completion.SetResult(true);
        }

        public async Task Process(GameManager manager)
        {
            Game = manager;
            await Process();
        }

        protected void Response(ActionResponseWrapper[] wrappers)
        {
            if (wrappers == null || wrappers.Length == 0)
                return;
            
            Game.Room.ResponseClientRpc(wrappers, UniqueId);
        }
        
        protected void Response(IActionResponse response)
        {
            var wrappers = ActionResponseWrapper.Package(response);
            Response(wrappers);
        }

        protected void Response(ActionResponseWrapper wrapper)
        {
            Response(wrapper.SingleArray());
        }
        
        protected void TargetResponse(ActionResponseWrapper[] wrappers, ulong target = ulong.MaxValue)
        {
            if (wrappers == null || wrappers.Length == 0)
                return;
            
            target = target == ulong.MaxValue ? RequesterId : target;
            var rpcParams = NetCodeMisc.RpcParamsWrapper(target); 
            Game.Room.ResponseClientRpc(wrappers, UniqueId, rpcParams);
        }
        
        protected void TargetResponse(IActionResponse response, ulong target = ulong.MaxValue)
        {
            var wrappers = ActionResponseWrapper.Package(response);
            TargetResponse(wrappers, target);
        }
        
        protected void TargetResponse(ActionResponseWrapper wrapper, ulong target = ulong.MaxValue)
        {
            TargetResponse(wrapper.SingleArray(), target);
        }
        
        public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Unique);
            serializer.SerializeValue(ref Requester);
        }

        public virtual bool Equals(BaseRequest other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return UniqueId.Equals(other.UniqueId) && RequesterId == other.RequesterId;
        }
    }
}