using UnityEngine.Localization.Components;

public class SubCardItem : AbstractInformationComponent
{
    public LocalizeStringEvent nameEvent;
    public LocalizeStringEvent descriptionEvent;
    public KeywordReplacer keywords;
    
    public override void SetInformation<T>(T data)
    {
        if (data is not string cardName)
            return;
        
        nameEvent.SetEntry("card_name_" + cardName[1..]);
        descriptionEvent.SetEntry("card_description_" + cardName[1..]);
        descriptionEvent.RefreshString();
    }
}
