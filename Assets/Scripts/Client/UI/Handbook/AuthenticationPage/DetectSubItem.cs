using UnityEngine;

public class DetectSubItem : AbstractBookPage
{
    [Header("Self Components")] 
    public LoginPage parent;
    public WaitingText waitingText;
    
    public async void OnEnable()
    {
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