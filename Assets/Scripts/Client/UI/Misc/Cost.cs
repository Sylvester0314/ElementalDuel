using System;
using Shared.Classes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cost : AbstractInformationComponent
{
    public Image type;
    public TextMeshProUGUI count;

    private string _suffix;
    
    public void Initialize(float scale, string suffix)
    {
        _suffix = suffix;
        transform.localScale = Vector3.one * scale;
    }
    
    public override async void SetInformation<T>(T data)
    {
        if (data is not ValueTuple<CostUnion, Color> tuple)
            return;
        
        var (cost, color) = tuple;
        var path = ResourceLoader.GetCostSpritePath(cost.type, _suffix);
        var sprite = await ResourceLoader.LoadSprite(path);
        
        if (gameObject == null)
            return;
        
        if (type != null)
            type.sprite = sprite;
        
        if (count != null)
        {
            count.color = color;
            count.text = cost.count.ToString();
        }
    }
}
