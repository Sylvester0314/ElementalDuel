using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DarkMask : MonoBehaviour
{
    public Material mask;
    
    public List<Image> imgTargets = new ();
    public List<TextMeshProUGUI> tmpTargets = new ();
    
    private readonly Dictionary<int, Color> _colors = new ();

    public void Awake()
    {
        tmpTargets.ForEach(text => AppendText(text, true));
    }

    public void AppendText(TextMeshProUGUI text, bool isInit = false)
    {
        var id = text.gameObject.GetInstanceID();
        _colors.Add(id, text.color);
        
        if (!isInit)
            tmpTargets.Add(text);
    }
    
    public void SetActive(bool active)
    {
        var maskImg = active ? mask : null;
        
        imgTargets.ForEach(image => image.material = maskImg);
        
        tmpTargets.ForEach(text =>
        {
            var id = text.gameObject.GetInstanceID();
            var color = _colors[id] * (active ? 0.45f : 1);
            color.a = _colors[id].a;
            text.color = color;
        });
    }
}