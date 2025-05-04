using Unity.Netcode;
using UnityEngine;

public class GeneralSettingPage : AbstractSettingPage
{
    public MiddleButton logoutButton;
    
    public void Awake()
    {
        settingLogic = new SettingLogic(settingsContainer, scroll);

        button.Callback = () => {};

        logoutButton.Callback = () =>
        {
            Debug.Log("Logout");
            if (NetworkManager.Singleton.IsConnectedClient)
                NetworkManager.Singleton.Shutdown();
        };
    }
}
