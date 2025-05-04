using System;
using System.Collections.Generic;
using Server.Logic.Event;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shared.Logic.Event
{
    public enum DamageModifier
    {
        None,
        Increase,
        Decrease,
        Reduce,
        Amplify,
        ElementalInfuse,
    }
    
    [Serializable]
    public class ModifyDamageEffect : BaseModifyEffect
    {
        public DamageModifier modifier;

        [ShowIf("Numerical"), Range(0, 100)] 
        public int value;

        [ShowIf("modifier", DamageModifier.ElementalInfuse)]
        public Element originalElement = Element.Physical;
        [ShowIf("modifier", DamageModifier.ElementalInfuse)]
        public Element convertedElement;
        
        public override List<BaseEvent> Modify(Status handler, IEventModifiable e)
        {
            if (e is not DamageEvent damage)
                return BaseEvent.EmptyList;

            if (Numerical)
            {
                if (modifier == DamageModifier.Decrease)
                {
                    if (damage.CalculateDamage() == 0)
                        return BaseEvent.EmptyList;
                    
                    damage.Modifiers.decrease += value;
                }
                
                if (modifier == DamageModifier.Increase)
                    damage.Modifiers.increase += value;
                
                if (modifier == DamageModifier.Amplify)
                    damage.Modifiers.amplify += value;
                
                if (modifier == DamageModifier.Reduce)
                    damage.Modifiers.reduce *= value;
                
                damage.TriggeredStatuses.Add(handler);
                
                return AutoConsume(handler);
            }

            if (modifier == DamageModifier.ElementalInfuse && damage.ElementType == originalElement)
            {
                damage.ElementType = convertedElement;
                damage.TriggeredStatuses.Add(handler);
                return AutoConsume(handler);
            }

            return BaseEvent.EmptyList;
        }
        
        private bool Numerical => modifier is not (DamageModifier.None or DamageModifier.ElementalInfuse);
    }
}