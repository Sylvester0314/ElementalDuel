using TMPro;
using UnityEngine;

public class EnterGameSubItem : AbstractBookPage
{
    [Header("Self Components")] 
    public LoginPage parent;
    public TextMeshProUGUI title;
    public MiddleButton enterButton;
    public MiddleButton loginButton;
    
    public void Start()
    {
        enterButton.Callback = Handbook.Instance.LoginSuccessfully;
        loginButton.Callback = () => parent.SwitchDisplaySubpage(2);
    }

    public override void Open()
    {
        base.Open();
        var pattern = ResourceLoader.GetLocalizedUIText("account_detected");
        var username = NakamaManager.Instance.Session.Username;
        title.text = pattern.Replace("{Name}", username);
    }
}