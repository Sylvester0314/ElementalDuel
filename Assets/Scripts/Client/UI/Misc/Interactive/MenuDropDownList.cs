using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuDropDownList : MonoBehaviour
{
    public bool isOpening;
    public bool clickingSub;
    public List<MenuSubButton> subs;

    public void Awake()
    {
        subs.ForEach(sub => sub.parent = this);
    }

    public void SetSubButtonCallback(string key, Action callback)
    {
        var button = subs.Find(x => x.key == key);
        button.Callback = callback;
    }
    
    public void SwitchStatus()
    {
        SetStatus(!isOpening);
    }
    
    public void SetStatus(bool status)
    {
        isOpening = status;
        gameObject.SetActive(status);
    }
}