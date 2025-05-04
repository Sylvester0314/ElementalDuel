using System;
using Shared.Enums;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

public class CharacterAsset : ScriptableObject, IComparable<ActionCardAsset>, ICardAsset, IInitializableScriptableObject
{
    [FoldoutGroup("Display"), PropertyOrder(0)]
    public string characterName;
    [FoldoutGroup("Display"), PropertyOrder(1), ShowInInspector, ReadOnly, LabelText(" ")]
    public string PreviewName => ResourceLoader.GetLocalizedCard(characterName);
    
    [FoldoutGroup("Display"), PropertyOrder(2), PropertySpace(2)]
    public float avatarPivotY = 0.75f;
    [FoldoutGroup("Display"), PropertyOrder(3), PreviewField(Height = 50)]
    public Sprite cardImage;
    
    [FoldoutGroup("Configurations"), Range(5, 15)]
    public int baseMaxHealth = 10;
    [FoldoutGroup("Configurations"), Range(1, 5)]
    public int baseMaxEnergy = 2;
    [FoldoutGroup("Configurations")]
    public List<Property> properties;
    
    public List<SkillAsset> skillList;

    public List<Property> Properties => properties;
    public string Name => characterName;

    public int CompareTo(ActionCardAsset obj)
    {
        var self = int.Parse(name.Split("-")[0]);
        var other = int.Parse(obj.name.Split("-")[0]);

        return other.CompareTo(self);
    }

    public async void Initialize(string fileName)
    {
        var realName = fileName.Split('-')[0];
        characterName = "card_name_" + realName;

        var element = realName[1] switch
        {
            '1' => Property.ElementCryo,
            '2' => Property.ElementHydro,
            '3' => Property.ElementPyro,
            '4' => Property.ElementElectro,
            '6' => Property.ElementGeo,
            '7' => Property.ElementDendro,
            '5' => Property.ElementAnemo,
            _ => Property.Physical
        };
        var weapon = realName[0] == '2' ? Property.WeaponNone  : Property.WeaponBow;
        var nation = realName[0] == '2' ? Property.CampMonster : Property.NationMondstadt;
        
        properties = new List<Property> { element, weapon, nation };
        
        var facePath = $"Assets/Sources/Characters/Character_Cardface_{realName}.png";
        cardImage = await ResourceLoader.LoadSprite(facePath);

        skillList = new List<SkillAsset>();
        
        for (var i = 0; i < 3; i++)
        {
            var skillName = $"{realName}{i + 1}";
            var skill = CreateInstance<SkillAsset>();
            skill.Initialize(skillName);
            
            var assetPath = $"Assets/SOAssets/Skills/new/{skillName}.asset";
            AssetDatabase.CreateAsset(skill, assetPath);
            
            skillList.Add(skill);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}