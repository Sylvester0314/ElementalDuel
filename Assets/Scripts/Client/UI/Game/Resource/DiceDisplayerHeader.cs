using Shared.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceDisplayerHeader : MonoBehaviour
{
    public TextMeshProUGUI count;
    public Image background;
    public Image glow;
    public Image pointer;
    
    [Header("Self Configurations")]
    public Sprite selfBackground;
    public Color selfColor;
    public Sprite selfPointer;
    
    [Header("Opponent Configurations")]
    public Sprite oppoBackground;
    public Color oppoColor;
    public Sprite oppoPointer;

    public void Initialize(Site site)
    {
        pointer.sprite = site == Site.Self ? selfPointer : oppoPointer;
        background.sprite = site == Site.Self ? selfBackground : oppoBackground;
        glow.color = site == Site.Self ? selfColor : oppoColor;
    }
}