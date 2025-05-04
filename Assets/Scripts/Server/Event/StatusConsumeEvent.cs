using System.Collections.Generic;
using Client.Logic.Response;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Logic.Statuses;
using Shared.Misc;

namespace Server.Logic.Event
{
    public class StatusConsumeEvent : BaseEvent
    {
        public bool IsDiscarded;
        public Status Status;
        
        public StatusConsumeEvent(IEventSource source, IEventTarget target, IEventGenerator via)
            : base(source, target, via) { }

        public static StatusConsumeEvent Discard(Status status)
            => new (status, null, null) { Status = status, IsDiscarded = true };

        public static StatusConsumeEvent Create(Status status)
            => new (status, null, null) { Status = status, IsDiscarded = false };
        
        public override List<BaseEvent> Execute(ResolveTree resolve)
        {
            if (IsDiscarded)
                Status.Parent.Remove(Status.UniqueId);

            resolve.Events.Add(this);
            
            return EmptyList;
        }

        public override IReadOnlyList<IActionResponse> ToResponses()
            => IsDiscarded
                ? StatusResponse.Discard(Status).SingleList()
                : StatusResponse.Consume(Status).SingleList();

        public override void Log() => Logger
            .AppendLine(IsDiscarded ? "Discarded" : "Consumed")
            .AppendLine(" Status Information:")
            .Append(Status.CreateLogger())
            .Print();
    }
}