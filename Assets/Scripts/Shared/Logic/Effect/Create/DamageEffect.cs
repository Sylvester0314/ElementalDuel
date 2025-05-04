using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Server.Logic.Event;
using Shared.Handler;
using Shared.Logic.Statuses;
using Sirenix.OdinInspector;

namespace Shared.Logic.Effect
{
    [Serializable]
    public class DamageEffect : AttributeModifiableEffect
    {
        [BoxGroup("Effect Configurations")]
        public Element elementType;
        [BoxGroup("Effect Configurations")]
        public int damageAmount;
        [BoxGroup("Effect Configurations")]
        public bool mainTarget;

        protected override IReadOnlyList<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via)
        {
            var character = (via as SkillLogic)?.Owner;
            var targets = GetTargets(source.Belongs, character);
            var damageType = DamageType.None;

            if (via is SkillLogic skill)
                damageType = skill.Type switch
                {
                    SkillType.NormalAttack   => DamageType.NormalAttack,
                    SkillType.ElementalSkill => DamageType.ElementalSkill,
                    SkillType.ElementalBurst => DamageType.ElementalBurst,
                    _                        => DamageType.None
                };
            if (source is Status status)
                damageType = status.Type switch
                {
                    StatusType.CombatStatus or StatusType.Status
                        => DamageType.Status,
                    StatusType.Summon 
                        => DamageType.Summon,
                    _   => DamageType.None
                };
            
            return targets.Select(data => new DamageEvent(source, data, via)
            {
                Amount = damageAmount,
                IsMainTarget = mainTarget,
                ElementType = elementType,
                DamageTypes = damageType
            }).ToList();
        }
        
        public override IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e)
            => GenerateEvents(handler, e.Via).Concat(AutoConsume(handler)).ToList();
    }
}