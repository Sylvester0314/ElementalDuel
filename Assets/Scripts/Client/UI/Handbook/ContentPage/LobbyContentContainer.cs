using Client.UI.Misc.Transition;
using UnityEngine;

public class LobbyContentContainer : AbstractHandbookContainer
{
    [Header("Self References")]
    public Sprite buttonImage;

    [HideInInspector] 
    public SettingLogic logic;
    
    public void Update()
    {
        // when opening page is "create room" and user opened a drop-down list
        // after left click anywhere, close the drop-down list 
        if (!Input.GetMouseButtonDown(0) || choosingMark.index != 1 || logic.clickingSetting)
            return;

        var page = (CreateRoomPage)pages[1];
        page.settingLogic.editingSetting?.FadeOutDropDownList();
    }

    public override void Open()
    {
        base.Open();
        Handbook.Instance.ShowButton();
        Handbook.Instance.ButtonClickCallback = ClickButton;
        Handbook.Instance.buttonImage.sprite = buttonImage;

        FixedScene.Instance.Initialize();
    }

    private void ClickButton()
    {
        Handbook.Instance.SwitchContainerDisplay(2);
    }
}