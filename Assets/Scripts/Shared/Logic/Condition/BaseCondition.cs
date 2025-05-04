using System;
using Server.Logic.Event;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;

namespace Shared.Logic.Condition
{
    [Serializable]
    public abstract class BaseCondition
    {
        public abstract bool CheckCondition(BaseEvent e, EffectVariables vars, Status handler);
    }
}