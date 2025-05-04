using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceCount : MonoBehaviour
{
    public Image pointLight;
    public Image background;
    public Image front;
    public TextMeshProUGUI count;
    
    [Header("Configurations")]
    public List<Color> backgroundColors;
    public List<Color> frontColors;

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            count.text = _count.ToString();
        }   
    }
    private int _count;

    public void SetActiveStatus(bool active)
    {
        var index = active ? 1 : 0;
        
        pointLight.gameObject.SetActive(active);
        background.color = backgroundColors[index];
        front.color = frontColors[index];
    }
}