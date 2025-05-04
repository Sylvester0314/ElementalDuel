using System;
using System.Collections.Generic;
using Server.GameLogic;
using Server.Logic.Event;
using Shared.Enums;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;
using Shared.Misc;
using Sirenix.OdinInspector;

namespace Shared.Logic.Condition
{
    [Flags]
    public enum DamageSourceType 
    {
        None,
        Skill      = 1 << 0,
        Technique  = 1 << 1,
        Summon     = 1 << 2,
        Status     = 1 << 3
    }

    [Flags]
    public enum TargetType
    {
        Active   = 1 << 0,
        Standby  = 1 << 1,
        All      = Active | Standby
    }
    
    [Serializable]
    public class DamageAttributeCondition : BaseCondition
    {
        [BoxGroup("Source Restriction"), LabelText("Enable")]
        public bool restrictDamageSource;
        [BoxGroup("Source Restriction"), ShowIf("restrictDamageSource")]
        public Site sourceSite;
        [BoxGroup("Source Restriction"), ShowIf("restrictDamageSource")]
        public DamageSourceType sourceTypes;
        
        [BoxGroup("Target Restriction"), LabelText("Enable")]
        public bool restrictDamageTarget;
        [BoxGroup("Target Restriction"), ShowIf("restrictDamageTarget")]
        public TargetType targetType;
        
        [BoxGroup("Element Restriction"), LabelText("Enable")]
        public bool restrictElementType;
        [BoxGroup("Element Restriction"), ShowIf("restrictElementType")]
        public Element elementType;
        
        [BoxGroup("Damage Amount Restriction"), LabelText("Enable")]
        public bool restrictDamageAmount;
        [BoxGroup("Damage Amount Restriction"), ShowIf("restrictDamageAmount")]
        public CompareOperator compareOperator;
        [BoxGroup("Damage Amount Restriction"), ShowIf("restrictDamageAmount")]
        public int amount;
        
        [BoxGroup("Damage Type Restriction"), LabelText("Enable")]
        public bool restrictDamageTypes;
        [BoxGroup("Damage Type Restriction"), ShowIf("restrictDamageTypes")]
        public DamageType damageTypes;
        
        [BoxGroup("Reaction Restriction"), LabelText("Enable")]
        public bool restrictReactionTypes;
        [BoxGroup("Reaction Restriction"), ShowIf("restrictReactionTypes")]
        public List<ElementalReaction> reactions;
        
        public override bool CheckCondition(BaseEvent e, EffectVariables vars, Status handler)
        {
            if (e is not DamageEvent damage)
                return false;
            
            if (restrictElementType && damage.ElementType != elementType)
                return false;

            if (restrictDamageTypes && !damage.DamageTypes.HasFlag(damageTypes))
                return false;
                
            if (restrictDamageAmount && !StaticMisc.Compare(damage.Amount, amount, compareOperator))
                return false;
            
            if (restrictReactionTypes && !reactions.Contains(damage.Reaction))
                return false;

            if (!restrictDamageSource)
                return true;
            
            if (!CheckSite(damage, handler))
                return false;
            
            if (sourceTypes is DamageSourceType.None)
                return true;

            return damage.Source switch
            {
                CharacterData   => CheckSkillSource(damage.Via as SkillLogic),
                Status trigger  => CheckStatusSource(trigger, handler),
                _               => false
            };
        }

        private bool CheckSite(DamageEvent damage, Status handler)
        {
            if (sourceSite is Site.Both or Site.None)
                return true;

            var sourceId = damage.Source.Belongs.Id;
            var handlerId = handler.Belongs.Id;
            
            return (sourceId == handlerId) ^ (sourceSite is Site.Opponent);
        }

        private bool CheckSkillSource(SkillLogic skill)
        {
            if (skill == null)
                return false;
            
            if (sourceTypes.HasFlag(DamageSourceType.Skill) && skill.Type is SkillType.Technique)
                return false;
            
            if (sourceTypes.HasFlag(DamageSourceType.Technique) && skill.Type is not SkillType.Technique)
                return false;
            
            return true;
        }

        private bool CheckStatusSource(Status trigger, Status handler)
        {
            if (handler == null)
                return false;

            if (sourceTypes.HasFlag(DamageSourceType.Status) &&
                trigger.Type is not (StatusType.Status or StatusType.CombatStatus)
               )
                return false;

            if (sourceTypes.HasFlag(DamageSourceType.Summon) && trigger.Type is not StatusType.Summon)
                return false;

            return true;
        }
    }
}