using System.Collections.Generic;
using Shared.Enums;
using UnityEngine.UI;

public class ReactionPreviewItem : AbstractAppliedManager
{
    public Image background;

    protected override void ResetIcons(List<Image> images = null)
    {
        base.ResetIcons(images);
        background.gameObject.SetActive(false);
    }
    
    public void SetReaction(List<ElementalApplication> reaction)
    {
        ResetIcons();
        
        var count = reaction.Count;
        for (var i = 0; i < count; i++)
            SetElementIcon(i, reaction[i]);
        
        if (count == 2)
            background.gameObject.SetActive(true);
    }
}