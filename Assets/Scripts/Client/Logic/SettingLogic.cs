using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Misc;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SettingLogic
{
    public bool clickingSetting;
    public bool disableClick;
    
    [HideInInspector]
    public ScrollRect scroll;
    public SettingItem editingSetting;
    public Dictionary<string, SettingItem> Settings = new ();
    
    public SettingLogic(Transform container, ScrollRect scroll)
    {
        this.scroll = scroll;
        foreach (Transform child in container)
        {
            var setting = child.GetComponent<SettingItem>();
            setting.Initialize(this);
            Settings.Add(setting.asset.name.ToCamelCase(), setting);
        }
    }

    public T ToData<T>()
    {
        var dataStructure = Settings
            .ToDictionary(
                setting => setting.Key,
                setting => setting.Value.choosing.key
            );
        return JsonUtility.FromJson<T>(dataStructure.ToJson());
    }
}