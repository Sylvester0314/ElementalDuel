using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Logic.Response;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;

namespace Server.Logic.Event
{
    public abstract class BaseEvent
    {
        public static List<BaseEvent> EmptyList = Array.Empty<BaseEvent>().ToList();
        
        public Guid EventId;
        public IEventSource Source;
        public IEventTarget Target;
        public IEventGenerator Via;

        public PlayerLogic Trigger => Source.Belongs;
        public PlayerLogic Receiver => Target.Belongs;
        protected StringBuilder Logger => CreateLogger();
        
        protected BaseEvent(IEventSource source, IEventTarget target, IEventGenerator via)
        {
            EventId = Guid.NewGuid();
            Source = source;
            Target = target;
            Via = via;
        }
        
        public abstract List<BaseEvent> Execute(ResolveTree resolve);

        public abstract void Log();
        
        public virtual IReadOnlyList<IActionResponse> ToResponses() => new List<IActionResponse>();

        public BaseEvent UnifiedId(Guid id)
        {
            if (this is not StatusConsumeEvent)
                EventId = id;
            return this;
        }
        
        private StringBuilder CreateLogger()
        {
            var builder = new StringBuilder();

            return builder
                .AppendLine($"##### {GetType().Name} Attributes #####")
                .Append("EventId: ").AppendLine(EventId.ToString())
                .Append("Source: ").AppendLine(Source?.LocalizedName)
                .Append("Target: ").AppendLine(Target?.LocalizedName)
                .Append("Via Entity: ").AppendLine(Via?.LocalizedName)
                .AppendLine();
        }

        public void Deconstruct(out Guid id, out BaseEvent e)
        {
            id = EventId;
            e = this;
        }
    }

    public abstract class PreviewableEvent : BaseEvent
    {
        protected PreviewableEvent(IEventSource source, IEventTarget target, IEventGenerator via)
            : base(source, target, via) { }
        
        public abstract void WriteToOverview(ResolveOverview overview);
    }

    public abstract class AttributeModifiableEvent : PreviewableEvent
    {
        public int Amount;

        protected AttributeModifiableEvent(IEventSource source, IEventTarget target, IEventGenerator via)
            : base(source, target, via) { }
    }
}