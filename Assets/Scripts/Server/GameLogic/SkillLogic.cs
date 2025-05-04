using System.Collections.Generic;
using Server.Logic.Event;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Condition;
using Shared.Logic.Effect;

namespace Server.GameLogic
{
    public class SkillLogic : ICostHandler
    {
        public PlayerLogic Logic;
        public CharacterData Owner;
        
        public string Name;
        public bool IsHide;
        public bool Chargeable;
        public SkillType Type;
        public ConditionLogic UseCondition;
        public EffectVariables Variables;
        public EffectContainer Effects;

        // TODO 更多充能判断
        public bool CanGainEnergy => Chargeable;
        public string EntityName => Name;
        public string Key => $"{Owner?.UniqueId ?? string.Empty}_{Name}";
        public string LocalizedName => ResourceLoader.GetLocalizedValue("Skill", Name);
        public CostLogic Cost { get; private set; }
        public PlayerLogic Belongs => Logic;

        public SkillLogic() { }
        
        public SkillLogic(CharacterData owner, SkillAsset asset)
        {
            Owner = owner;
            Logic = Owner?.Logic.PlayerLogic;
            
            Name = asset.skillName;
            IsHide = asset.hide;
            Chargeable = asset.chargeable;
            Cost = new CostLogic(asset.costs);
            UseCondition = asset.useCondition;
            Effects = new EffectContainer(asset.Effects);
            Variables = new EffectVariables(new Dictionary<string, int>
            {
                { "GameUsedCount", 0 },
                { "RoundUsedCount", 0 }
            });
        }

        public static SkillLogic SwitchActive(PlayerLogic logic, SkillAsset asset)
            => new (null, asset) { Logic = logic };
        
        // TODO 时点实现 - 技能费用计算
        public void CalculateActualCost()
        {
            
        }

        public void AddCount()
        {
            Variables.Modify("GameUsedCount", 1);
            Variables.Modify("RoundUsedCount", 1);
        }

        public bool EvaluateUsable()
        {
            var passive = PassiveEvent.Create(Owner, this);
            return UseCondition.Evaluate(passive, EffectVariables.Empty);
        }

        public SkillLogic Clone(CharacterData owner, PlayerLogic logic = null)
            => new ()
            {
                Owner = owner,
                Logic = owner?.Logic.PlayerLogic ?? logic,
                IsHide = IsHide,
                Chargeable = Chargeable,
                Type = Type,
                Name = Name,
                Cost = Cost.Clone(),
                UseCondition = UseCondition,
                Effects = Effects,
                Variables = Variables.Clone()
            };
    }
}