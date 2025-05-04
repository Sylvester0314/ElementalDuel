using UnityEngine;

public class ContentSizeController : MonoBehaviour
{
    public RectTransform background;
    public RectTransform content;
    public RectTransform contentMask;
    public RectTransform information;
    public GameObject footer;

    private float _height = -1f;
    private const float MaxHeight = 168.8f;
    private Coroutine _waitFor;
    
    private void SizeFitter()
    {
        footer.SetActive(_height.Equals(MaxHeight));
        background.sizeDelta = new Vector2(background.sizeDelta.x, _height * 6.25f + 25);
        information.sizeDelta = new Vector2(information.sizeDelta.x, _height + 4);
        contentMask.sizeDelta = new Vector2(contentMask.sizeDelta.x, _height);
    }
    
    public void Update()
    {
        var baseHeight = Mathf.Min(content.rect.height, MaxHeight);
        if (Mathf.Approximately(_height, baseHeight))
            return;
        
        _height = baseHeight;
        SizeFitter();
    }
}
