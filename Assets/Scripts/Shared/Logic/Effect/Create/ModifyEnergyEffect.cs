using System;
using System.Collections.Generic;
using System.Linq;
using Server.Logic.Event;
using Shared.Handler;
using Shared.Logic.Statuses;

namespace Shared.Logic.Effect
{
    public enum EnergyModifyMode
    {
        Gain,
        Loss
    }
    
    [Serializable]
    public class ModifyEnergyEffect : AttributeModifiableEffect
    {
        public EnergyModifyMode modifyMode;
        public int amount;

        protected override IReadOnlyList<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via)
        {
            var value = (modifyMode == EnergyModifyMode.Gain ? 1 : 0) * amount;
            var targets = GetTargets(source.Belongs);
                
            return targets
                .Select(data => new ModifyEnergyEvent(source, data, via, value))
                .ToList();
        }

        public override IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e)
            => GenerateEvents(handler, e.Via).Concat(AutoConsume(handler)).ToList();
    }
}