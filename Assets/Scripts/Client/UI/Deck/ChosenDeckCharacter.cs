using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChosenDeckCharacter : AbstractDeckCard, IPointerEnterHandler
{
    public GameObject emptyStatus;
    public CanvasGroup hoverLight;
    public CanvasGroup clickLight;
    public Image avatar;
    public RectTransform avatarRect;

    public CharacterAsset Asset { get; private set; }
    
    // -1 means character has been chosen
    private int _chosenCount;
    
    public int Count
    {
        get => _chosenCount;
        set
        {
            _chosenCount = value;
            var area = BuildDeckContainer.Instance.chosenCharacterArea;
            
            area.SetChoosingCount(value, Asset);
            area.RefreshCount();

            if (value != 0)
                return;

            Asset = null;
            emptyStatus.SetActive(true);
            avatar.gameObject.SetActive(false);
        }
    }
    
    public void SetAvatarDisplay(CharacterAsset asset)
    {
        Asset = asset;
        Count = -1;

        avatar.sprite = Asset.cardImage;
        avatarRect.pivot.Set(0.5f, Asset.avatarPivotY);
    }
    
    protected override void OnLeftClick()
    {
        if (Asset == null)
            return;
        
        var global = BuildDeckContainer.Instance;
        var assets = global.chosenCharacterArea.slots
            .Where(chosen => chosen.Asset != null)
            .Select(chosen => chosen.Asset)
            .OfType<ICardAsset>()
            .ToList();

        global.information.Open(assets, Asset);
    }

    // Deselect this character
    protected override void OnRightClick()
    {
        if (Asset == null)
            return;
        
        Count = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Flash(hoverLight, 1);
        DOVirtual.DelayedCall(0.125f, () => Flash(hoverLight, 0));
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Flash(clickLight, 1);
        DOVirtual.DelayedCall(0.125f, () => Flash(clickLight, 0));
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
    }

    private void Flash(CanvasGroup canvas, float alpha)
    {
        var ease = alpha == 0 ? Ease.OutSine : Ease.OutExpo;
        var duration = alpha == 0 ? 0.15f : 0.1f;
        canvas.DOFade(alpha, duration).SetEase(ease);
    }
}