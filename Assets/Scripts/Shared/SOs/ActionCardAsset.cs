using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Client.Logic.BuildCondition;
using Server.GameLogic;
using Shared.Classes;
using Shared.Logic.Condition;
using Shared.Logic.Effect;
using Shared.Misc;
using Sirenix.OdinInspector;
using UnityEngine;

public class ActionCardAsset : DescriptionDrawerScriptableObject, IComparable<ActionCardAsset>, IInitializableScriptableObject, ICardAsset
{
    #region Display

    [FoldoutGroup("Display"), PropertyOrder(0)]
    public string cardName;

    [FoldoutGroup("Display"), PropertyOrder(1), ShowInInspector, ReadOnly, LabelText(" ")]
    public string PreviewName => ResourceLoader.GetLocalizedCard(cardName);
    
    [FoldoutGroup("Display"), PropertyOrder(2), PropertySpace(2)]
    public string description;

    [FoldoutGroup("Display"), PropertyOrder(3), ShowInInspector, ReadOnly, LabelText(" ")]
    [CustomValueDrawer("CustomDescriptionDrawer")]
    public string PreviewDesc => ResourceLoader.GetLocalizedCard(description);

    [FoldoutGroup("Display"), PropertyOrder(4), PropertySpace(2)]
    [HideIf("IsSummon")]
    public string bannerHint;

    [FoldoutGroup("Display"), PropertyOrder(5), ShowInInspector, ReadOnly, LabelText(" ")]
    [HideIf("@IsSummon || bannerHint.Length == 0")]
    public string PreviewHint => ResourceLoader.GetLocalizedUIText(bannerHint);
    
    [FoldoutGroup("Display"), PropertyOrder(6), PropertySpace(2)]
    [HideIf("IsSummon")]
    public Sprite cardSnapshot;

    [FoldoutGroup("Display"), PropertyOrder(7), PreviewField(Height = 50)]
    public Sprite cardImage;

    #endregion
    
    [FoldoutGroup("Configurations")]
    public ActionCardType cardType;
    [FoldoutGroup("Configurations"), PropertySpace(2), SerializeReference]
    [HideIf("IsSummon")]
    public List<BaseBuildCondition> buildCondition;
    [FoldoutGroup("Configurations"), PropertySpace(2)]
    [HideIf("IsSummon")]
    public List<Property> properties;
    [FoldoutGroup("Configurations"), PropertySpace(2)]
    [HideIf("IsSummon")]
    public List<CostUnion> costs;
    
    [BoxGroup("Condition and Effects")]
    [HideIf("IsSummon")]
    public ConditionLogic useCondition;
    [BoxGroup("Condition and Effects")]
    [HideIf("IsSummon")]
    public List<EffectLogic<BaseEffect>> effects;

    [HideInInspector] 
    public bool isValid;
    
    public List<Property> Properties => properties;
    public string Name => cardName;
    public List<EffectLogic<BaseEffect>> Effects => effects
        .Select(logic => logic.Contravariance()).ToList();
    
    public bool IsValid(List<CharacterAsset> characters)
    {
        return buildCondition.TryGetValue(0)?.CheckCondition(characters) ?? true;
    }
    
    public ActionCardAsset CheckValidity(List<CharacterAsset> characters)
    {
        isValid = IsValid(characters);
        return this;
    }

    public List<string> BannerHint(PlayerLogic logic)
    {
        if (bannerHint != string.Empty)
            return bannerHint.SingleList();
        
        var mainEntry = "play_card_banner";
        var subReference = "CardDescription";
        var subEntry = cardName;
        
        if (cardType is ActionCardType.Equipment)
        {
            mainEntry = "use_equipment_card_banner";
            subReference = "UIText";
            subEntry = properties.Contains(Property.CardRelic)
                ? "property_card_relic"
                : WeaponSlotEntry();
        }
        else if (properties.Contains(Property.CardTalent))
        {
            if (cardType is ActionCardType.Event)
                mainEntry = "affect_action_talent_banner";
            else
                mainEntry = properties.Contains(Property.CardAction)
                    ? "use_talent_card_banner"
                    : "equip_talent_banner";

            subEntry = logic.ActiveCharacter.Name;
        }

        return new List<string> { mainEntry, subReference, subEntry };
    }

    public int CompareTo(ActionCardAsset obj)
    {
        var validComparison = isValid.CompareTo(obj.isValid);
        if (validComparison != 0) 
            return validComparison;
        
        var self = int.Parse(name.Split("-")[0]);
        var other = int.Parse(obj.name.Split("-")[0]);
        
        return other.CompareTo(self);
    }

    public async void Initialize(string fileName)
    {
        var realName = fileName.Split('-')[0];
        cardName = "card_name_" + realName;
        description = "card_description_" + realName;
        
        costs = new List<CostUnion>();
        properties = new List<Property>();
        bannerHint = string.Empty;
        useCondition = new ConditionLogic();
        effects = new List<EffectLogic<BaseEffect>>();
        
        var condition = new EmptyBuildCondition();

        var facePath = $"Assets/Sources/ActionCards/Action_Card_{realName}.png";
        cardImage = await ResourceLoader.LoadSprite(facePath);
        
        if (realName[0] == '1')
        {
            cardType = ActionCardType.Summon;
            return;
        }
        
        if (realName[0] == '2')
        {
            Properties.Add(Property.CardTalent);
            
            var element = (Element)char.GetNumericValue(realName[2]);
            var cost = element.ToCostType();
            costs.Add(new CostUnion(cost, 3));
        }
        else
            costs.Add(new CostUnion(CostType.Same, 0));
        
        if (realName[1] == '1')
        {
            cardType = ActionCardType.Equipment;
            condition.conditionDescription = Equipment(realName[2], realName[3]);
        }

        if (realName[1] == '2')
        {
            cardType = ActionCardType.Support;

            var supportType = realName[2] switch
            {
                '1' => Property.CardLocation,
                '2' => Property.CardAlly,
                '3' => Property.CardItem,
                _ => Property.CardLocation
            };
            Properties.Add(supportType);
        }

        if (realName[1] == '3')
        {
            cardType = ActionCardType.Event;

            if (realName[2] == '0')
            {
                Properties.Add(Property.CardLegend);
                condition.conditionDescription = "build_limit_legend";
            }
            if (realName[2] == '1')
                Properties.Add(Property.CardSync);
            if (realName[2] == '3')
            {
                Properties.Add(Property.CardFood);
                condition.conditionDescription = "build_limit_food";
            }
        }

        if (condition.conditionDescription != string.Empty)
            buildCondition = new List<BaseBuildCondition> { condition };

        var snapPath = $"Assets/Sources/CardSnapshots/Action_Card_{realName}.jpg";
        cardSnapshot = await ResourceLoader.LoadSprite(snapPath);
    }

    private string Equipment(char third, char fourth)
    {
        if (third == '2')
        {
            Properties.Add(Property.CardRelic);
            return "build_limit_relic";
        }
            
        if (third == '1')
        {
            var weaponType = fourth switch
            {
                '1' => Property.WeaponCatalyst,
                '2' => Property.WeaponBow,
                '3' => Property.WeaponClaymore,
                '4' => Property.WeaponPole,
                '5' => Property.WeaponSword,
                _ => Property.WeaponNone
            };
                
            var typeStr = weaponType
                .ToSnakeCase().ToLower()
                .Split('_')[1];
                
            Properties.Add(Property.CardWeapon);
            Properties.Add(weaponType);
            return $"build_limit_{typeStr}";
        }

        if (third == '3')
        {
            Properties.Add(Property.CardTechnique);
            return "build_limit_technique";
        }
        
        return string.Empty;
    }

    private string WeaponSlotEntry()
    {
        var weaponProperties = new List<Property>
        {
            Property.WeaponBow,
            Property.WeaponSword,
            Property.WeaponPole,
            Property.WeaponClaymore,
            Property.WeaponCatalyst
        };
        
        var type = properties
            .Where(property => weaponProperties.Contains(property))
            .ToList();

        return type.FirstOrDefault() switch
        {
            Property.WeaponBow      => "property_weapon_bow",
            Property.WeaponSword    => "property_weapon_sword",
            Property.WeaponPole     => "property_weapon_pole",
            Property.WeaponClaymore => "property_weapon_claymore",
            Property.WeaponCatalyst => "property_weapon_catalyst",
            _                       => "property_card_weapon"
        };
    }

    private bool IsSummon => cardType == ActionCardType.Summon;
}