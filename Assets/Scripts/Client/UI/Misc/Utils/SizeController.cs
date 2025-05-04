using UnityEngine;

[ExecuteAlways]
public class SizeController : MonoBehaviour
{
    public RectTransform parent;
    public float horizontalScale = 1f;
    public float verticalScale = 1f;
    
    private Rect _lastRect;
    private RectTransform _self;
    
    public void Awake()
    {
        _self = GetComponent<RectTransform>();
        
        if (parent == null)
            parent = transform.parent.GetComponent<RectTransform>();
    }

    public void Update()
    {
        if (parent.rect == _lastRect)
            return;
        var rect = parent.rect;
        
        _self.sizeDelta = new Vector2(
          rect.size.x * horizontalScale,
          rect.size.y * verticalScale
        );
        _lastRect = rect;
    }
}
