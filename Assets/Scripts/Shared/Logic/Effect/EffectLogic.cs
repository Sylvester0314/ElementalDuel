using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Logic.Condition;
using Shared.Logic.Effect;
using Shared.Logic.Effect.Modify;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EffectLogic<T> where T : BaseEffect
{
    public ConditionLogic condition;

    [PropertySpace(2), SerializeReference]
    public List<T> subEffects = new ();

    public static EffectLogic<BaseEffect> ConsumeDurationEffect
        => new ()
        {
            condition = new ConditionLogic(),
            subEffects = new List<BaseEffect> { new ConsumeDurationEffect() }
        };
    
    public EffectLogic<BaseEffect> Contravariance()
        => new ()
        {
            condition = condition,
            subEffects = subEffects.Cast<BaseEffect>().ToList()
        };
}