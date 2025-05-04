using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractSettingPage : AbstractBookPage
{
    [Header("Setting Components")]
    public SettingLogic settingLogic;
    public Transform settingsContainer;
    public ScrollRect scroll;
    public MiddleButton button;
}