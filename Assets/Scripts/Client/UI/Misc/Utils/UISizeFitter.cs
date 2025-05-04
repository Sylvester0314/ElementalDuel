using UnityEngine;

[ExecuteAlways]
public class UISizeFitter : MonoBehaviour
{
    public RectTransform rootCanvas;
    public float baseWidth;
    public float baseHeight;
    public float baseScale = 1f;
    public float compareRatio = 16 / 9f;
    
    private Vector2 _size;

    public void Update()
    {
        if (rootCanvas.rect.size == _size)
            return;
        
        _size = rootCanvas.rect.size;
        var ratio = _size.x / _size.y;
        var scale = ratio.CompareTo(compareRatio) < 0
            ? _size.x / baseWidth : _size.y / baseHeight;
        
        transform.localScale = Vector3.one * (scale * baseScale);
    }
}