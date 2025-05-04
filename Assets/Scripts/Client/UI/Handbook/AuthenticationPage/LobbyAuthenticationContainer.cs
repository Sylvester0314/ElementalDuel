using Client.UI.Misc.Transition;

public class LobbyAuthenticationContainer : AbstractHandbookContainer
{
    public override void Open()
    {
        base.Open();
        Handbook.Instance.HideButton();
        FixedScene.Instance.Close();
    }
}