using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FilterSet : MonoBehaviour
{
    public FilterItem choosingItem;
    public List<FilterItem> children;

    public Action<string> OnChoosingChanged;
    
    public void Start()
    {
        foreach (var child in children)
            child.parent = this;
        
        var rect = gameObject.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
}