using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractSettingPage : AbstractBookPage
{
    public LobbySettingContainer settingContainer;

    [Header("Setting Components")]
    public SettingLogic settingLogic;
    public Transform settingsContainer;
    public ScrollRect scroll;
    public MiddleButton button;
    
    public void OnEnable()
    {
        if (settingContainer != null)
            settingContainer.setting = settingLogic;
    } 
}