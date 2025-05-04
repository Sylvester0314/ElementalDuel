using System.Collections.Generic;
using Server.Logic.Event;
using Shared.Handler;
using Shared.Logic.Statuses;

namespace Shared.Logic.Effect.Modify
{
    public class ConsumeDurationEffect : BaseModifyEffect
    {
        public override List<BaseEvent> Modify(Status handler, IEventModifiable _)
            => handler.ConsumeDuration();
    }
}