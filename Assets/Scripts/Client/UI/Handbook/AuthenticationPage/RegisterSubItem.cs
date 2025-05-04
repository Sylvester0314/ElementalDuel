using TMPro;
using UnityEngine;

public class RegisterSubItem : AbstractBookPage
{
    [Header("Self Components")]
    public LoginPage parent;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public MiddleButton anotherButton;
    public MiddleButton confirmButton;

    private string _username;
    private string _password;
    
    public void Awake()
    {
        usernameInput.onValidateInput = parent.UsernameValidate;
        passwordInput.onValidateInput = parent.PasswordValidate;
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        
        confirmButton.Callback = ConfirmRegister;
        anotherButton.Callback = () => parent.SwitchDisplaySubpage(2);
    }

    public override void Open()
    {
        base.Open();
        usernameInput.text = string.Empty;
        passwordInput.text = string.Empty;
    }

    public async void ConfirmRegister()
    {
        _username = usernameInput.text;
        _password = passwordInput.text;
        
        if (_username.Length == 0 || _password.Length == 0)
            return;

        var success = await NakamaManager.Instance.CreateNewAccount(
            _username, _password,
            RegisterErrorCatch
        );

        if (success)
            Handbook.Instance.LoginSuccessfully();
    }

    public void RegisterErrorCatch(string message)
    {
        if (message.Contains("8 characters"))
        {
            Handbook.Instance.popUps
                .Create<PopUps>("pop_hint")
                .AppendText("password_length_limit")
                .Display();
            return;
        }

        if (message.Equals("account_registered"))
        {
            Handbook.Instance.popUps
                .Create<PopUps>("pop_hint")
                .AppendText("account_registered")
                .Display();
            return;
        }
        
        Handbook.Instance.popUps
            .Create<PopUps>("pop_error")
            .AppendError($"Input username: {_username}")
            .AppendError($"Input password: {_password}")
            .AppendError($"Error message: {message}")
            .Display();
    }
}