using System;
using Server.Logic.Event;
using Shared.Enums;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;

namespace Shared.Logic.Condition
{
    [Serializable]
    public class AttachedOnlyCondition : BaseCondition
    {
        public EntityType type;

        public override bool CheckCondition(BaseEvent e, EffectVariables vars, Status handler)
        {
            if (handler.Type is not StatusType.Status)
                return false;
            
            var entity = type == EntityType.Source ? e.Source : e.Target;
            return entity.Belongs.Id == handler.Belongs.Id;
        }
    }
}