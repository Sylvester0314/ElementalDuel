using Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using Shared.Classes;
using Shared.Logic.Condition;
using Shared.Logic.Effect;
using Shared.Misc;
using Sirenix.OdinInspector;
using UnityEngine;

public class SkillAsset : DescriptionDrawerScriptableObject, IInitializableScriptableObject
{
    #region Display

    [FoldoutGroup("Display"), PropertyOrder(0)]
    public string skillName;

    [FoldoutGroup("Display"), PropertyOrder(1), ShowInInspector, ReadOnly, LabelText(" ")]
    public string PreviewName => ResourceLoader.GetLocalizedValue("Skill", skillName);
    
    [FoldoutGroup("Display"), PropertyOrder(2), PropertySpace(2)]
    public string description;

    [FoldoutGroup("Display"), PropertyOrder(3), ShowInInspector, ReadOnly, LabelText(" ")]
    [CustomValueDrawer("CustomDescriptionDrawer")]
    public string PreviewDesc => ResourceLoader.GetLocalizedValue("Skill", description);
    
    [FoldoutGroup("Display"), PropertyOrder(4), PreviewField(Height = 50)]
    public Sprite icon;
    
    #endregion

    [FoldoutGroup("Configurations")] 
    public bool hide;
    [FoldoutGroup("Configurations")] 
    public bool chargeable;
    [FoldoutGroup("Configurations")]
    public SkillType skillType;
    [FoldoutGroup("Configurations")]
    public Element element = Element.None;
    [FoldoutGroup("Configurations")]
    public int damage = -1;

    [FoldoutGroup("Configurations")]
    public List<CostUnion> costs;
    
    [BoxGroup("Condition and Effects")]
    public ConditionLogic useCondition;
    [BoxGroup("Condition and Effects")]
    public List<EffectLogic<BaseCreateEffect>> effects;

    public List<EffectLogic<BaseEffect>> Effects => effects
        .Select(logic => logic.Contravariance()).ToList();

    public void Initialize(string fileName)
    {
        var realName = fileName.Split('-')[0];
        var type = realName[^1];
        skillName = "skill_name_" + realName;
        description = type == '1'
            ? "skill_description_normal_attack"
            : "skill_description_" + realName;
        skillType = type switch
        {
            '1' => SkillType.NormalAttack,
            '2' => SkillType.ElementalSkill,
            '3' => SkillType.ElementalBurst,
            _ => SkillType.PassiveSkill
        };

        if (skillType == SkillType.PassiveSkill)
            return;

        damage = type == '1' ? 2 : 3;
        element = (Element)char.GetNumericValue(realName[1]);
        useCondition = new ConditionLogic();
        var cost = element.ToCostType();
        
        costs = new List<CostUnion> { new (cost, type == '1' ? 1 : 3) };
        if (type == '1')
            costs.Add(new CostUnion(CostType.Diff, 2));
        if (type == '3')
            costs.Add(new CostUnion(CostType.Energy, 2));
            
        var damageEvent = new DamageEffect
        {
            elementType = element,
            damageAmount = damage
        };
        var effect = new EffectLogic<BaseCreateEffect> {
            subEffects = new List<BaseCreateEffect> { damageEvent }
        };
        effects = new List<EffectLogic<BaseCreateEffect>> { effect };
    }
}