using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ScalerFitter : MonoBehaviour
{
    public RectTransform rootCanvas;
    public float baseWidth;
    public float baseHeight;
    public float baseScale = 1f;
    public float compareRatio = 16 / 9f;
    public CanvasScaler canvasScaler;
    
    private Vector2 _size;

    public void Update()
    {
        if (rootCanvas.rect.size == _size)
            return;
        
        _size = rootCanvas.rect.size;
        var ratio = _size.x / _size.y;
        var scale = ratio.CompareTo(compareRatio) < 0
            ? _size.x * canvasScaler.scaleFactor / baseWidth 
            : _size.y * canvasScaler.scaleFactor / baseHeight;

        canvasScaler.scaleFactor = scale * baseScale;
    }
}
