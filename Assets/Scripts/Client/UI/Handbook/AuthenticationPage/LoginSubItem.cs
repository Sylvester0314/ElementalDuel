using TMPro;
using UnityEngine;

public class LoginSubItem : AbstractBookPage
{
    [Header("Self Components")]
    public LoginPage parent;
    public TextMeshProUGUI title;
    public TMP_InputField uidInput;
    public TMP_InputField passwordInput;
    public MiddleButton confirmButton;
    public MiddleButton anotherButton;
    
    private string _uid;
    private string _password;
    
    public void Awake()
    {
        uidInput.characterValidation = TMP_InputField.CharacterValidation.Digit;
        passwordInput.onValidateInput = parent.PasswordValidate;
        passwordInput.contentType = TMP_InputField.ContentType.Password;

        confirmButton.Callback = ConfirmLogin;
        anotherButton.Callback = () => parent.SwitchDisplaySubpage(1);
    }
    
    public override void Open()
    {
        base.Open();
        uidInput.text = string.Empty;
        passwordInput.text = string.Empty;
    }

    public async void ConfirmLogin()
    {
        _uid = uidInput.text;
        _password = passwordInput.text;
        
        if (_uid.Length != 5 || _password.Length == 0)
            return;
        
        var success = await NakamaManager.Instance.LoginAccount(_uid, _password, LoginErrorCatch);
        if (success)
            Handbook.Instance.LoginSuccessfully();
    }

    public void LoginErrorCatch(string message)
    {
        if (message.Contains("8 characters"))
        {
            Handbook.Instance.popUps
                .Create<PopUps>("pop_hint")
                .AppendText("password_length_limit")
                .Display();
            return;
        }
        
        if (message.Contains("not found"))
        {
            Handbook.Instance.popUps
                .Create<PopUps>("pop_hint")
                .AppendText("not_exist_account")
                .Display();
            return;
        }
        
        if (message.Contains("Invalid credentials"))
        {
            Handbook.Instance.popUps
                .Create<PopUps>("pop_hint")
                .AppendText("wrong_uid_or_password")
                .Display();
            return;
        }
        
        Handbook.Instance.popUps
            .Create<PopUps>("pop_error")
            .AppendError($"Input UID: {_uid}")
            .AppendError($"Input password: {_password}")
            .AppendError($"Error message: {message}")
            .Display();
    }
}