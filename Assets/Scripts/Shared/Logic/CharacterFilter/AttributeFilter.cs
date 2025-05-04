using System;
using System.Collections.Generic;
using Server.GameLogic;
using Shared.Enums;
using Shared.Misc;

namespace Shared.Logic.CharacterFilter
{
    public enum AttributeType
    {
        Health,
        Energy
    }
    
    public enum AttributeFilterMode
    {
        CurrentValue,
        MaxValue,
        DiffToMaxValue
    }
    
    [Serializable]
    public class AttributeFilter : BaseFilter
    {
        public AttributeType attributeType;
        public AttributeFilterMode attributeMode;
        public CompareOperator compareOperator;
        public int compareValue;
        
        private static Dictionary<(AttributeType, AttributeFilterMode), Func<CharacterData, int>> _getters = new ()
        {
            { (AttributeType.Health, AttributeFilterMode.CurrentValue),   c => c.CurrentHealth },
            { (AttributeType.Health, AttributeFilterMode.MaxValue),       c => c.MaxHealth },
            { (AttributeType.Health, AttributeFilterMode.DiffToMaxValue), c => c.Damaged },
            { (AttributeType.Energy, AttributeFilterMode.CurrentValue),   c => c.CurrentEnergy },
            { (AttributeType.Energy, AttributeFilterMode.MaxValue),       c => c.MaxEnergy },
            { (AttributeType.Energy, AttributeFilterMode.DiffToMaxValue), c => c.MaxEnergy - c.CurrentEnergy }
        };
        
        public override bool Check(CharacterData character)
        {
            if (!_getters.TryGetValue((attributeType, attributeMode), out var getter))
                return false;
            
            return StaticMisc.Compare(getter(character), compareValue, compareOperator);
        }
    }
}