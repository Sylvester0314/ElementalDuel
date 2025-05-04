using System;
using System.Collections.Generic;
using DG.Tweening;
using Shared.Handler;
using Shared.Classes;
using Shared.Misc;
using UnityEngine;
using UnityEngine.Localization.Components;

public class DeckListContainer : MonoBehaviour, ISceneUnloadHandler
{
    [Header("In Game Data")] 
    public int maxPage;
    public int currentPage;
    public DeckData currentData;
    public DeckItem choosingItem;
    
    [Header("Components")] 
    public SceneLoader sceneLoader;
    public MiddleButton closeButton;
    public GameObject cover;
    public CanvasGroup hintCanvas;
    public LocalizeStringEvent hintEvent;
    public List<DeckItem> deckItems;
    public CanvasGroup leftArrow;
    public CanvasGroup rightArrow;
    public PopUpsInjector popUpsInjector;
    
    private PrepareRoom _prepareRoom;
    private List<List<DeckData>> _decks;
    private Tween _delayClose;
    
    public PrepareRoom Room => _prepareRoom;

    public void Awake()
    {
        _prepareRoom = FindObjectOfType<PrepareRoom>();
        
        closeButton.Callback = () => _prepareRoom.sceneLoader.UnloadCurrentScene();
        RefreshDeckList(false, onComplete: _prepareRoom.sceneLoader.ActiveFlag);

        foreach (var deck in deckItems)
            deck.OnClicking = EditDeck;
    }

    public void OnSceneUnload()
    {
        RefreshDeckList(true);
    }

    #region Deck Item Operation

    private void EditDeck(DeckData data)
    {
        currentData = data;
        sceneLoader.LoadScene("BuildScene", lazyLoad: true);
    }

    public async void SetActiveDeck(DeckItem deckItem)
    {
        var deckList = _prepareRoom.currentDecks;
        var prevActive = deckList.Find(deck => deck.isUsing);
        
        prevActive.isUsing = false;
        deckItem.data.isUsing = true;
        
        deckItem.usingIcon.SetActive(true);
        deckItems.Find(item => item.data == prevActive)
            ?.usingIcon.SetActive(false);

        _prepareRoom.RefreshSelfCharacters();

        DisplayHint("set_as_active_deck_success");
        
        await NakamaManager.Instance.WriteDeckStorage(deckList);
    }

    public async void AddDeck(DeckData deck)
    {
        await NakamaManager.Instance.SaveDeck(deck);

        RefreshDeckList(true, _decks[currentPage].Count);
        DisplayHint("deck_copied");
        
        // If the new deck is in the new page, switch the showing page
        if (currentPage != maxPage)
            DisplayDecks(maxPage);
    }

    public async void RemoveDeck(DeckData deck)
    {
        var isUsing = deck.isUsing;
        _prepareRoom.currentDecks.RemoveAll(data => data.UniqueId == deck.UniqueId);
        if (isUsing)
            _prepareRoom.currentDecks[0].isUsing = true;
        
        RefreshDeckList(false);
        await NakamaManager.Instance.WriteDeckStorage(_prepareRoom.currentDecks);
    }

    public int GetDeckCount()
    {
        return _prepareRoom.currentDecks.Count;
    }
    
    #endregion

    #region Deck List Display

    public async void RefreshDeckList(bool reload, int switchPage = -1, Action onComplete = null)
    {
        if (reload)
            await _prepareRoom.UpdateDecks();

        _decks = _prepareRoom.currentDecks.Paginate(10);
        maxPage = _decks.Count - 1;
        
        // after delete a deck, if current page has no deck
        // switch to the first deck
        var page = switchPage switch
        {
            -1 => currentPage > maxPage ? 0 : currentPage,
            _ => switchPage == 10 ? maxPage : currentPage
        };
        DisplayDecks(page);

        onComplete?.Invoke();
    }
    
    private void DisplayDecks(int page)
    {
        currentPage = page;
        var decks = _decks[currentPage];
        
        leftArrow.gameObject.SetActive(currentPage != 0);
        rightArrow.gameObject.SetActive(currentPage != maxPage);
        
        var count = decks.Count;
        for (var i = 0; i < deckItems.Count; i++)
        {
            var data = i < count ? decks[i] : null;
            deckItems[i].SetStyle(data);
        }
    }

    public void ResetChoosingItem()
    {
        cover.gameObject.SetActive(false);
        choosingItem?.CancelChoosingStatus();
        choosingItem = null;
    }
    
    public void DisplayHint(string entry)
    {
        hintEvent.SetEntry(entry);
        hintCanvas.alpha = 0;
        _delayClose?.Kill();
        
        hintCanvas.DOFade(1, 0.4f).SetEase(Ease.OutExpo);
        _delayClose = DOVirtual.DelayedCall(
            1.6f,
            () => hintCanvas
                .DOFade(0, 0.3f)
                .SetEase(Ease.OutExpo)
        );
    }

    #endregion

    #region Page Switch

    public void OnArrowDown(bool isLeft)
    {
        var arrow = isLeft ? leftArrow : rightArrow;
        arrow.DOFade(0.5f, 0.2f).SetEase(Ease.OutSine);
    }
    
    public void OnArrowUp(bool isLeft)
    {
        var arrow = isLeft ? leftArrow : rightArrow;
        arrow.DOFade(1, 0.2f).SetEase(Ease.OutSine);
    }

    public void OnArrowClick(int delta)
    {
        var target = currentPage + delta;
        if (target > maxPage || target < 0)
            return;
        
        DisplayDecks(target);
    }

    #endregion
}