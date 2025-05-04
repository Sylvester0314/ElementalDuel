using System;
using Shared.Handler;
using Shared.Logic.Statuses;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public enum StatusLifecycle
    {
        Generate,
        Consume,
        Discard
    }
    
    public class StatusResponse : BaseResponse, IEquatable<StatusResponse>
    {
        public string UniqueId;
        public Status Status;
        public StatusLifecycle Lifecycle;
        
        public StatusResponse()
        {
            Status = new Status();
        }

        public StatusResponse(string id, Status status, StatusLifecycle lifecycle)
        {
            UniqueId = id;
            Status = status;
            Lifecycle = lifecycle;
        }

        #region Factory Methods

        public static StatusResponse Generate(string zoneId, Status status)
            => new (zoneId, status, StatusLifecycle.Generate);

        public static StatusResponse Discard(Status status)
            => new (status.UniqueId, status, StatusLifecycle.Discard);
            
        public static StatusResponse Consume(Status status)
            => new (status.UniqueId, status, StatusLifecycle.Consume);
        
        #endregion
        
        public override async void Process()
        {
            if (Lifecycle == StatusLifecycle.Generate)
                await Global.StatusContainers[UniqueId].Append(Status);
            else if (Global.EntitiesMap[UniqueId] is IStatusEntity entity)
            {
                if (Lifecycle == StatusLifecycle.Consume)
                    entity.RefreshLiftHint(Status);
                if (Lifecycle == StatusLifecycle.Discard)
                    entity.Discard();
            }
            
            base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref UniqueId);
            serializer.SerializeValue(ref Status);
            serializer.SerializeValue(ref Lifecycle);
        }

        public bool Equals(StatusResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return UniqueId == other.UniqueId &&
                   Lifecycle == other.Lifecycle &&
                   Status.Equals(other.Status);
        }
    }
}