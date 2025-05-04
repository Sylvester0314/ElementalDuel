using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class TagItem : MonoBehaviour
{
    public Image background;
    public LocalizeStringEvent tagContentEvent;
    public TextMeshProUGUI tagContent;

    public List<Color> colors;

    public void Initialize(ValueTuple<string, int, bool> data)
    {
        if (data.Item3)
            tagContentEvent.SetEntry(data.Item1);
        else
            tagContent.text = data.Item1;
    
        background.color = colors[data.Item2];
    }
}