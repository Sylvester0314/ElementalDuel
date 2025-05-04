using UnityEngine;

public class LobbySettingContainer : AbstractHandbookContainer
{
    [Header("Self References")]
    public Sprite buttonImage;
    
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