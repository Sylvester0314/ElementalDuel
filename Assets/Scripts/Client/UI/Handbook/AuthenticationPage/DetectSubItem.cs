using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class DetectSubItem : AbstractBookPage
{
    [Header("Self Components")] 
    public LoginPage parent;
    public WaitingText waitingText;
    
    public async void OnEnable()
    {
        var lang = PlayerPrefs.GetString("Language");
        var locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code == lang);
        
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
        
        waitingText.Active("device_detecting");
        
        await NakamaManager.Instance.CheckDeviceAccount();
        
        var index = NakamaManager.Instance.Session == null ? 1 : 3;
        parent.SwitchDisplaySubpage(index);
    }

    public void OnDisable()
    {
        waitingText.Inactive();
    }
}