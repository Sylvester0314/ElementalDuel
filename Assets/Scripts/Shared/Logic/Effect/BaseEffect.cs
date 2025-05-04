using System;
using System.Collections.Generic;
using Server.Logic.Event;
using Shared.Logic.Statuses;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Shared.Logic.Effect
{
    [Serializable]
    public abstract class BaseEffect
    {
        [ShowIf("IsOwnedByStatus")]
        public bool autoConsume;
        
        public abstract IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e);
        
        private bool IsOwnedByStatus(InspectorProperty property)
        {
            var current = property?.Parent;

            while (current != null)
            {
                if (current.ValueEntry?.TypeOfValue == typeof(TimingEffectPair))
                    return true;

                current = current.Parent;
            }

            return false;
        }

        protected List<BaseEvent> AutoConsume(Status status)
            => autoConsume ? status.ConsumeUsage(1) : BaseEvent.EmptyList;
    }
}