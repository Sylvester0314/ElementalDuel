using System.Collections.Generic;
using Client.Logic.Request;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelfCountdown : AbstractTurnCountdown, 
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Declare End")] 
    public RectTransform declareEnd;
    public ConfirmButton button;
    public Hint hint;
    
    [Header("Display Components")]
    public CanvasGroup lightCanvas;
    public Image lights;
    public Image decoration;
    public Image outline;
    public Image background;
    public Image icon;
    
    [Header("Configurations")]
    public List<Color> decorationColors;
    public List<Color> outlineColors;
    public List<Color> backgroundColors;
    public List<Color> iconColors;
    public List<Color> lightColors;
    
    protected const float AnimationDuration = 0.15f;

    public void Awake()
    {
        button.Callback = DeclareEnd;
    }

    public void HideDeclareButton()
    {
        declareEnd.gameObject.SetActive(false);
    }
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        declareEnd.gameObject.SetActive(true);

        hint.hint.alpha = 0;
        // hint.hint.DOFade(1, 0.3f).SetEase(Ease.OutCubic);
    }

    private void DeclareEnd()
    {
        HideDeclareButton();
        
        var request = new DeclareEndRequest();
        var wrapper = ActionRequestWrapper.Create(request);
        parent.global.manager.RequestServerRpc(wrapper);
    }

    private void SetColor(int index)
    {
        decoration.color = decorationColors[index];
        outline.color = outlineColors[index];
        background.color = backgroundColors[index];
        icon.color = iconColors[index];

        if (index >= 1)
            lights.color = lightColors[index - 1];
    }

    private void Animation(float scale, float alpha)
    {
        transform.DOScale(Vector3.one * scale, AnimationDuration).SetEase(Ease.OutExpo);
        lights.DOFade(alpha, AnimationDuration).SetEase(Ease.OutExpo);
    }
    
    private void Reset()
    {
        SetColor(0);
        Animation(1, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetColor(1);
        Animation(1.045f, 1);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetColor(2);
        Animation(0.95f, 1);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Reset();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Reset();
    }
}