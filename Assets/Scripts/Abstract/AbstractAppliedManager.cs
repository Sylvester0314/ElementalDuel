using System.Collections.Generic;
using Shared.Enums;
using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractAppliedManager : MonoBehaviour
{
    [Header("Components")]
    public List<Image> icons;

    protected virtual void ResetIcons(List<Image> images = null)
    {
        images ??= icons;
        images.ForEach(icon => icon.gameObject.SetActive(false));
    }

    protected async void SetElementIcon(
        int index, ElementalApplication type, 
        List<Image> images = null, string iconType = "Element"
    )
    {
        if (type == ElementalApplication.None)
            return;

        images ??= icons;
        
        var path = $"Assets/Sources/UI/Elements/{iconType}_{type.ToString()}.png";
        var sprite = await ResourceLoader.LoadSprite(path);
        
        images[index].sprite = sprite;
        images[index].gameObject.SetActive(true);
    }
}