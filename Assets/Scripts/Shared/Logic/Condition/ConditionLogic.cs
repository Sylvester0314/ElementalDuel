using System;
using System.Collections.Generic;
using System.Linq;
using Server.Logic.Event;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;
using Shared.Misc;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shared.Logic.Condition
{
    public enum ConditionEvaluationMode
    {
        None,
        Any,
        All,
        AtLeastCount,
        Otherwise
    }

    [Serializable]
    public class ConditionLogic
    {
        public ConditionEvaluationMode evaluation = ConditionEvaluationMode.None;

        [ShowIf("evaluation", ConditionEvaluationMode.AtLeastCount)]
        public int count;

        [ShowIf("evaluation", ConditionEvaluationMode.Otherwise)]
        public int preposedIndex;
        
        [ShowIf("FactorEditable"), SerializeReference]
        public List<BaseCondition> factors = new ();

        public bool Evaluate(
            BaseEvent e, 
            EffectVariables vars, 
            bool[] availabilities = null, 
            Status handler = null
        )
        {
            if (evaluation == ConditionEvaluationMode.Otherwise && availabilities != null)
                return !availabilities.TryGetValue(preposedIndex);
            
            if (evaluation == ConditionEvaluationMode.None || factors.Count == 0)
                return true;

            var evaluated = factors
                .Select(condition => condition.CheckCondition(e, vars, handler))
                .Where(v => v)
                .ToList().Count;
            var evaluationNumber = evaluation switch
            {
                ConditionEvaluationMode.AtLeastCount => count,
                ConditionEvaluationMode.All          => factors.Count,
                ConditionEvaluationMode.Any          => 1,
                _                                    => 0
            };

            return evaluated >= evaluationNumber;
        }

        private bool FactorEditable =>
            evaluation is not (ConditionEvaluationMode.None or ConditionEvaluationMode.Otherwise);
    }   
}