using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldHide : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button toggle;
    public Image icon;
    public Sprite hideIcon;
    public Sprite showIcon;

    private bool _hide = true;

    public void OnEnable()
    {
        icon.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        icon.gameObject.SetActive(false);
    }

    public void Start()
    {
        toggle.onClick.AddListener(Toggle);;
    }

    public void Toggle()
    {
        _hide = !_hide;
        
        var sprite = _hide ? hideIcon : showIcon;
        var type = _hide 
            ? TMP_InputField.ContentType.Password
            : TMP_InputField.ContentType.Standard;

        icon.sprite = sprite;
        inputField.contentType = type;
        inputField.Select();
        inputField.ActivateInputField();
    }
}
