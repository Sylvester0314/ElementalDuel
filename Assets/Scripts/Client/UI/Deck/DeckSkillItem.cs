using Server.GameLogic;
using Shared.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DeckSkillItem : MonoBehaviour
{
    public Image icon;
    public LocalizeStringEvent skillType;
    public LocalizeStringEvent skillName;
    public KeywordReplacer description;
    public TextMeshProUGUI descriptionText;
    public CostSetComponent costSet;

    public void SetInformation(SkillAsset asset)
    {
        icon.sprite = asset.icon;
        skillName.SetEntry(asset.skillName);
        skillType.SetEntry(asset.skillType switch
        {
            SkillType.NormalAttack => "skill_name_type_normal_attack",
            SkillType.ElementalSkill => "skill_name_type_elemental_skill",
            SkillType.ElementalBurst => "skill_name_type_elemental_burst_nu",
            SkillType.PassiveSkill => "skill_name_type_passive_skill_nu",
            _ => string.Empty
        });
        
        var rawText = ResourceLoader.GetLocalizedValue("Skill", asset.description);
        descriptionText.text = description.ProcessStringWithSkillData(
            rawText, asset.damage, asset.element);

        var logic = new CostLogic(asset.costs);
        costSet.InitializeCostList("_Small");
        logic.RefreshCostDisplay(costSet);
    }
}
