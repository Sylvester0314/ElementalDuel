using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Shared.Classes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckItem : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
{
    public GameObject displayCards;
    public GameObject createIcon;
    public GameObject usingIcon;
        
    [Header("Sub Components")]
    public CanvasGroup lights;
    public List<DeckCharacterCard> cards;
    public TextMeshProUGUI deckName;
    public MenuDropDownList dropdown;
    public RectTransform choosingIcon;
    
    [Header("Others Components")]
    public CanvasGroup claspClose;
    public CanvasGroup claspOpen;
    public MiddleButton expendButton;

    [Header("References")] 
    public Sprite cardBack;

    public Action<DeckData> OnClicking;
    public const float Duration = 0.3f;
    public DeckData data;
    
    private Tween _loop;
    private DeckListContainer _container;

    public void Start()
    {
        _container = FindObjectOfType<DeckListContainer>();

        expendButton.Callback = () =>
        {
            dropdown.gameObject.SetActive(true);
            choosingIcon.gameObject.SetActive(true);
            
            _container.choosingItem = this;
            _container.cover.SetActive(true);
            _loop = choosingIcon.DOAnchorPosY(12, 0.6f)
                .SetEase(Ease.OutSine)
                .SetLoops(-1, LoopType.Yoyo);
        };
        
        dropdown.SetSubButtonCallback("active", SetDeckActive);
        // dropdown.SetSubButtonCallback("edit", EditDeckName);
        // dropdown.SetSubButtonCallback("design", EditDeckDesign);
        // dropdown.SetSubButtonCallback("preview", PreviewDeck);
        dropdown.SetSubButtonCallback("copy", CopyDeck);
        dropdown.SetSubButtonCallback("delete", DeleteDeck);
    }

    public void SetStyle(DeckData deck)
    {
        data = deck;

        var status = data == null;
        createIcon.SetActive(status);
        displayCards.SetActive(!status);
        expendButton.gameObject.SetActive(!status);
        usingIcon.SetActive(data?.isUsing ?? false);
        
        if (status)
            return;
        
        SetCardFace(0);
        SetCardFace(1);
        SetCardFace(2);
        deckName.text = data.deckName;
        
        return;

        void SetCardFace(int index)
        {
            var character = data.characters[index];
            var card = cards[index];
            
            var isEmpty = character == null;
            // TODO 用卡组配置的牌背进行展示
            card.cardFace.sprite = isEmpty ? cardBack : character.cardImage;
            card.cardFrame.gameObject.SetActive(!isEmpty);
        }
    }

    #region Dropdown Operation

    private void SetDeckActive()
    {
        if (data.isUsing)
            _container.DisplayHint("already_set_as_active_deck");
        else if (!data.CheckSaveFeasibility())
            _container.DisplayHint("deck_condition_not_met");
        else
            _container.SetActiveDeck(this);
        
        _container.ResetChoosingItem();
    }

    private void EditDeckName()
    {
        _container.popUpsInjector
            .Create<AdvancedPopUps>("edit_deck_name")
            .SetEntry()
            .SetAsyncCallbacks(ModifyDeckName)
            .AppendInputField(
                "enter_deck_name", 
                deckName.text
            )
            .Display();
        
        _container.ResetChoosingItem();
    }

    private async Task ModifyDeckName(List<string> results)
    {
        var newName = results[0];
        if (newName.Length != 0)
        {
            deckName.text = newName;
            await NakamaManager.Instance.RenameDeck(data.UniqueId, newName);
            _container.RefreshDeckList(true);
            return;
        }
        
        DOVirtual.DelayedCall(0.2f, () =>
        {
            var pop = _container.popUpsInjector
                .Create<AdvancedPopUps>("pop_hint")
                .AppendText("deck_name_cannot_be_empty");
            if (pop is AdvancedPopUps adv)
                adv.cancelButton.gameObject.SetActive(false);
            pop.Display();
        });
    }

    private void EditDeckDesign()
    {
        Debug.Log("Edit Deck Design");
        _container.ResetChoosingItem();
    }

    private void PreviewDeck()
    {
        Debug.Log("Preview Deck");
        _container.ResetChoosingItem();
    }

    private void CopyDeck()
    {
        _container.AddDeck(data.Copy());
        _container.ResetChoosingItem();
    }

    private void DeleteDeck()
    {
        if (_container.GetDeckCount() == 1)
        {
            _container.DisplayHint("cannot_delete_last_deck");
            _container.ResetChoosingItem();
            return;
        }
        
        _container.popUpsInjector
            .Create<AdvancedPopUps>("delete_deck")
            .SetCallbacks(_ => _container.RemoveDeck(data))
            .AppendText("confirm_delete_deck")
            .Display();

        _container.ResetChoosingItem();
    }

    public void CancelChoosingStatus()
    {
        _loop?.Kill();
        choosingIcon.gameObject.SetActive(false);
        dropdown.SetStatus(false);;
        Animation(0);
    }

    #endregion
    
    #region Interactive and Animation

    public void OnPointerEnter(PointerEventData eventData)
    {
        Animation(1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_container.choosingItem == this)
            return;
        
        Animation(0);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Animation(-1);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(Vector3.one, Duration).SetEase(Ease.OutExpo);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicking.Invoke(data);
        Animation(0);
    }

    private void Animation(int value)
    {
        // Enter = 1, Exit = 0
        var scale = Vector3.one * (1 + value * 0.04f);

        value = value <= 0 ? 0 : 1;
        transform.DOScale(scale, Duration).SetEase(Ease.OutExpo);
        lights.DOFade(value, Duration).SetEase(Ease.OutExpo);
        
        claspClose.DOFade(value ^ 1, Duration * 2).SetEase(Ease.OutExpo);
        claspOpen.DOFade(value, Duration * 2).SetEase(Ease.OutExpo);
        
        cards[0].Move("f", value);
        cards[1].Move("m", value);
        cards[2].Move("b", value);
    }

    #endregion
}