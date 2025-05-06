using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class GeneralSettingPage : AbstractSettingPage
{
    public MiddleButton logoutButton;
    
    public void Awake()
    {
        settingLogic = new SettingLogic(settingsContainer, scroll);

        button.Callback = () =>
        {
            if (!settingLogic.Settings.TryGetValue("language", out var setting))
                return;

            var lang = setting.choosing.key;
            var locale = LocalizationSettings.AvailableLocales.Locales
                .FirstOrDefault(l => l.Identifier.Code == lang);

            if (locale == null)
                return;

            PlayerPrefs.SetString("Language", lang);
            LocalizationSettings.SelectedLocale = locale;
        };

        logoutButton.Callback = () =>
        {
            Debug.Log("Logout");
            if (NetworkManager.Singleton.IsConnectedClient)
                NetworkManager.Singleton.Shutdown();
        };
    }
}
