using System.Collections.Generic;
using DG.Tweening;
using Server.ResolveLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthPreview : MonoBehaviour
{
    public Image icon;
    public Image background;
    public RectTransform backgroundRect;
    public CanvasGroup canvas;
    public TextMeshProUGUI healthDelta;
    
    [Header("References")]
    public List<Sprite> specialIcons;
    public List<Sprite> backgrounds;
 
    [Header("Configurations")]
    public Vector3 startPosition;
    public float endPositionX;
    public List<Color> textColors;
    public List<Color> backgroundColors;

    private Tween _tween;
    private readonly (float left, float right, float top, float bottom) _offset
        = (-0.9f, -0.95f, -0.25f, -0.25f);

    private Vector4 Base => new (_offset.left, 0, _offset.right, 0);
    private Vector4 Target => new (_offset.left, _offset.top, _offset.right, _offset.bottom);

    private const float Duration = 0.2f;
    
    public void Reset()
    {
        backgroundRect.offsetMax = new Vector2(_offset.right, 0);
        backgroundRect.offsetMin = new Vector2(_offset.left, 0);
        background.color = backgroundColors[0];
        icon.gameObject.SetActive(false);
        canvas.alpha = 0.5f;
        transform.localPosition = startPosition;
        gameObject.SetActive(false);
    }
    
    public void Open(CharacterModification modification)
    {
        Reset();
        
        if (!modification.HealReceived && !modification.DamageTook)
            return;
        
        if (modification.Defeated)
        {
            icon.sprite = specialIcons[0];
            icon.gameObject.SetActive(true);
            background.color = backgroundColors[1];
        }

        // If the character's health is not increased, and they are not received heal
        // They are considered to have only been damaged, and use the damage background
        var isDamage = modification.HealthModified <= 0 && !modification.HealReceived;
        var bgIndex = isDamage ? 0 : 1;
        background.sprite = backgrounds[bgIndex];
        
        var textColorIndex = bgIndex != 0 ? 2 : modification.Defeated ? 1 : 0;
        var healthModified = modification.HealthModified;
        healthDelta.color = textColors[textColorIndex];
        healthDelta.text = HealthModifyString(healthModified, isDamage);
        
        gameObject.SetActive(true);
        Animation();
    }

    public void Animation()
    {
        _tween?.Kill();
        _tween = DOTween.Sequence()
            .Append(transform.DOLocalMoveX(endPositionX, Duration).SetEase(Ease.OutCubic))
            .Join(canvas.DOFade(1, Duration).SetEase(Ease.OutCubic))
            .Join(DOTween.To(
                () => Base, v =>
                {
                    backgroundRect.offsetMin = new Vector2(v.x, v.w);
                    backgroundRect.offsetMax = new Vector2(-v.z, -v.y);
                },
                Target, Duration
            ).SetEase(Ease.OutCubic))
            .Play();
    }

    public static string HealthModifyString(int value, bool isDamage)
        => value switch
        {
            < 0 => value.ToString(),
            > 0 => $"+{value}",
            _   => (isDamage ? "-" : "+") + "0"
        };
}