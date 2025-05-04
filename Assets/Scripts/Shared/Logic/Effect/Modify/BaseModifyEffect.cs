using System;
using System.Collections.Generic;
using Server.Logic.Event;
using Shared.Handler;
using Shared.Logic.Statuses;

namespace Shared.Logic.Effect
{
    [Serializable]
    public abstract class BaseModifyEffect : BaseEffect
    {
        public override IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e)
        {
            if (e is IEventModifiable modifiableEvent)
                return Modify(handler, modifiableEvent);
            
            return BaseEvent.EmptyList;
        }
        
        public abstract List<BaseEvent> Modify(Status handler, IEventModifiable e);
    }
}