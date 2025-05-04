using System;
using Server.GameLogic;
using Server.Logic.Event;
using Shared.Enums;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;

namespace Shared.Logic.Condition
{
    public enum EntityType
    {
        Target,
        Source
    }
    
    [Serializable]
    public class ActiveOnlyCondition : BaseCondition
    {
        public EntityType type;
        
        public override bool CheckCondition(BaseEvent e, EffectVariables vars, Status handler)
        {
            var entity = type == EntityType.Source ? e.Source : e.Target;
            if (entity.Belongs.Id != handler.Belongs.Id)
                return false;

            if (handler.Type is StatusType.Status)
                return handler.Parent.Belongs.IsActive;

            if (entity is CharacterData character)
                return character.IsActive;

            return false;
        }
    }
}