using UnityEngine;

public class LobbySettingContainer : AbstractHandbookContainer
{
    [Header("Self References")]
    public Sprite buttonImage;
    
    [HideInInspector]
    public SettingLogic setting;
    
    public void Update()
    {
        if (!Input.GetMouseButtonDown(0) || setting.clickingSetting)
            return;

        var index = choosingMark.index;
        var page = (AbstractSettingPage)pages[index];
        page.settingLogic.editingSetting?.FadeOutDropDownList();
    }
    
    public override void Open()
    {
        base.Open();
        Handbook.Instance.ShowButton();
        Handbook.Instance.ButtonClickCallback = ClickButton;
        Handbook.Instance.buttonImage.sprite = buttonImage;
    }

    private void ClickButton()
    {
        Handbook.Instance.SwitchContainerDisplay(1);
    }
}