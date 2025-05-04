using Server.Logic.Event;
using Shared.Enums;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;

namespace Shared.Logic.Condition
{
    public class CompareCondition : BaseCondition
    {
        public CompareOperator Operator;

        public override bool CheckCondition(BaseEvent e, EffectVariables vars, Status handler)
        {
            return true;
        }
    }
}