using Server.Logic.Event;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;

namespace Shared.Logic.Condition
{
    public class ExistCondition : BaseCondition
    {
        public bool Negative;

        public override bool CheckCondition(BaseEvent e, EffectVariables vars, Status handler)
        {
            return true;
        }
    }
}