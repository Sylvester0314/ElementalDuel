using UnityEngine;
using UnityEngine.UI;

public class EquipController : MonoBehaviour
{
    public Material material;
    
    private Image _image;
    private bool _flash;
    private bool _showing;
    
    public void Start()
    {
        _image = GetComponent<Image>();
    }

    public void ToggleFlashStatus(bool flash)
    {
        _flash = flash;
        _image.material = _flash ? material : null;
    }

    public void ToggleShowingStatus(bool showing)
    {
        _showing = showing;
        gameObject.SetActive(_showing);
    }
}
