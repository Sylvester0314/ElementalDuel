using UnityEngine;
using UnityEngine.UI;

public class OperationSettingPage : AbstractSettingPage
{
    public void Awake()
    {
        settingLogic = new SettingLogic(settingsContainer, scroll);

        button.Callback = () => {};
    }
}