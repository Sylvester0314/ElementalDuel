using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.ResolveLogic;
using Shared.Enums;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class TimingEffectPair
{
    public Timing timing;
    public List<EffectLogic<BaseEffect>> effects = new ();
}

public class StatusCardAsset : DescriptionDrawerScriptableObject, IInitializableScriptableObject
{
    #region Display

    [FoldoutGroup("Display"), PropertyOrder(0)]
    public string statusName;

    [FoldoutGroup("Display"), PropertyOrder(1), ShowInInspector, ReadOnly, LabelText(" ")]
    public string PName => ResourceLoader.GetLocalizedCard(statusName);
    
    [FoldoutGroup("Display"), PropertyOrder(2), PropertySpace(2)]
    public string description;

    [FoldoutGroup("Display"), PropertyOrder(2), ShowInInspector, ReadOnly, LabelText(" ")]
    [CustomValueDrawer("CustomDescriptionDrawer")]
    public string PDesc => ResourceLoader.GetLocalizedCard(description);
    
    [FoldoutGroup("Display"), PropertyOrder(3), PreviewField(Height = 50)]
    public Sprite displayImage;
    
    [FoldoutGroup("Display"), PropertyOrder(4)]
    public string hintValueField = "usage";
    
    [FoldoutGroup("Display"), PropertyOrder(5)]
    public bool showHintIcon = true;
    
    [ShowIf("showHintIcon")]
    [FoldoutGroup("Display"), PropertyOrder(6), PreviewField(Height = 50)]
    public Sprite hintTypeIcon;
    
    [FoldoutGroup("Display"), PropertyOrder(7)]
    public bool showEventType;
    
    [ShowIf("showEventType")]
    [FoldoutGroup("Display"), PropertyOrder(8), PreviewField(Height = 50)]
    public Sprite eventTypeIcon;

    [ShowIf("showEventType")]
    [FoldoutGroup("Display"), PropertyOrder(9)]
    public int eventTypeValue;
    
    #endregion

    #region Config

    [FoldoutGroup("Configurations")] 
    public StatusType type;
    [ShowIf("@type != StatusType.Status && type != StatusType.CombatStatus")] 
    [FoldoutGroup("Configurations")]
    public ActionCardAsset relatedCard;
    [FoldoutGroup("Configurations")] 
    public List<Property> properties = new ();
    
    [FoldoutGroup("Life Mode")] 
    public StatusLifeMode mode;
    [FoldoutGroup("Life Mode")] 
    public bool autoDiscard = true;
    [FoldoutGroup("Life Mode")] 
    public bool canOverride;
    
    [BoxGroup("Life Mode/Usages"), HideLabel]
    [ShowIf("@(mode & StatusLifeMode.Usages) != StatusLifeMode.None")]
    public StatusLifeSetting usages = new (false);
    
    [BoxGroup("Life Mode/Durations"), HideLabel]
    [ShowIf("@(mode & StatusLifeMode.Durations) != StatusLifeMode.None")]
    public StatusLifeSetting durations = new (false);

    [BoxGroup("Life Mode/Round Limitation")]
    public bool restrictTriggerPerRound;
    
    [BoxGroup("Life Mode/Round Limitation")]
    [ShowIf("restrictTriggerPerRound"), Range(1, 10)]
    public int maxTriggersPerRound = 1;
    
    #endregion
    
    [BoxGroup("Effects"), ShowInInspector, OdinSerialize] 
    public SerializedDictionary<string, int> variables = new ();
    [BoxGroup("Effects"), ShowInInspector] 
    public List<TimingEffectPair> handlers = new ();
    
    private const string CommonStatusPath = "Assets/Sources/StatusCards/Status_Common_Buff.png";
    private const string HintPath = "Assets/Sources/UI/Lifes/Hint_Timer.png";

    public async void Initialize(string fileName)
    {
        var realName = fileName.Split('-')[0];
        statusName = "card_name_" + realName;   
        description = "card_description_" + fileName;

        var isSupport = realName[..2].Equals("32");
        var dir = isSupport ? "SupportCards" : "StatusCards";
        type = isSupport ? StatusType.Support : StatusType.Status;

        if (realName[..2].Equals("31"))
        {
            var equipType = realName[2] == '1' ? "Weapon" : "Relic";
            var path = $"Assets/Sources/UI/Lifes/Equip_{equipType}.png";
            displayImage = await ResourceLoader.LoadSprite(path);
            showHintIcon = false;
            relatedCard = await ResourceLoader.LoadSoAsset<ActionCardAsset>(realName);
            return;
        }
        
        hintTypeIcon = await ResourceLoader.LoadSprite(HintPath);
        
        await TryFindMainImage(dir, realName);
        if (displayImage != null)
        {
            if (type == StatusType.Status)
            {
                hintTypeIcon = null;
                showHintIcon = false;
            }
            return;
        }
        
        type = StatusType.Summon;
        await TryFindMainImage("SummonCards", realName);
        if (displayImage != null)
        {
            var element = realName[2] switch
            {
                '1' => Element.Cryo,
                '2' => Element.Hydro,
                '3' => Element.Pyro,
                '4' => Element.Electro,
                '6' => Element.Geo,
                '7' => Element.Dendro,
                '5' => Element.Anemo,
                _   => Element.Physical
            };
            var path = $"Assets/Sources/UI/Elements/Element_{element.ToString()}.png";
            showEventType = true;
            eventTypeIcon = await ResourceLoader.LoadSprite(path);
            return;
        }
        
        type = StatusType.Status;
        showHintIcon = false;
        displayImage = await ResourceLoader.LoadSprite(CommonStatusPath);
        hintTypeIcon = null;
    }

    private async Task TryFindMainImage(string dir, string id)
    {
        var displayPath = $"Assets/Sources/{dir}/Status_{id}.png";
        displayImage = await ResourceLoader.TryLoadSprite(displayPath);

        if (displayImage != null && dir != "StatusCards")
            relatedCard = await ResourceLoader.LoadSoAsset<ActionCardAsset>(id);
    }
}