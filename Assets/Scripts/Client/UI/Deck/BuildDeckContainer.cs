using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Server.Managers;
using Shared.Classes;
using Shared.Enums;
using Shared.Misc;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class BuildDeckContainer : MonoBehaviour
{
    public static BuildDeckContainer Instance;
    
    [Header("Components")]
    public FilterSet baseType;
    public FilterSet extendType;
    public TextMeshProUGUI deckNameText;
    public InventoryDeckArea inventoryArea;
    public ChosenDeckCardArea chosenCardArea;
    public ChosenCharacterArea chosenCharacterArea;
    
    [Header("Top/Button Area Components")] 
    public TextMeshProUGUI characterCount;
    public TextMeshProUGUI actionCardCount;
    public MiddleButton closeButton;
    public ConfirmButton saveButton;
    public ConfirmButton clearButton;
    public DeckCardInformation information;
    public Image editIcon;

    [Header("Hint Banner")] 
    public CanvasGroup hintCanvas;
    public LocalizeStringEvent hintEvent;
    public PopUpsInjector popUpsInjector;
    
    [Header("Configurations")] 
    public List<Property> characterTypes;
    public List<Property> actionCardTypes;

    private Tween _delayClose;
    private DeckListContainer _deckList;
    private NetworkRoom _room;

    private CardPoolPreset _pool;
    private DeckData _data;
    private string _preset;

    public async void Awake()
    {
        Instance = this;

        baseType.OnChoosingChanged = OnBaseTypeChanged;
        extendType.OnChoosingChanged = OnExtendTypeChanged;
        closeButton.Callback = ExitBuildMode;
        clearButton.Callback = ClearCurrentDeck;
        saveButton.AsyncCallback = SaveDeck;

        _deckList = FindObjectOfType<DeckListContainer>();
        _room = FindObjectOfType<NetworkRoom>();
        _data = _deckList?.currentData;
     
        // Load card pool preset data
        var preset = _data == null 
            ? _room?.config.Value.cardPoolPreset ?? "weird"
            : _data.basePreset;
        _preset = preset.Replace("card_pool_preset_", "");
        _pool = await ResourceLoader.LoadSoAsset<CardPoolPreset>(_preset);
        
        baseType.children[0].Initialize("Character");
        baseType.children[1].Initialize("Action");
        
        var deckName = _data?.deckName ?? ResourceLoader.GetLocalizedUIText("default_deck_key");
        await ModifyDeckName(deckName);
    }

    public void Start()
    {
        StartCoroutine(InitializeDeckStatus());
    }

    private IEnumerator InitializeDeckStatus()
    {
        yield return new WaitUntil(() => _pool != null);
        
        // Load current deck data
        LoadDeckData();
        
        // Choose the first selection
        baseType.children.First().Choose();
        
        // Finish scene load
        _deckList.sceneLoader.ActiveFlag();
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

    #region Deck Name Function
    
    public void EditDeckName()
    {
        popUpsInjector
            .Create<AdvancedPopUps>("edit_deck_name")
            .SetEntry()
            .SetAsyncCallbacks(OnConfirmModifyDeckName)
            .AppendInputField(
                "enter_deck_name", 
                deckNameText.text
            )
            .Display();
    }
    
    public void OnPointerDownOnDeckName()
    {
        editIcon.DOFade(0.5f, 0.3f).SetEase(Ease.OutExpo);
    }
    
    public void OnPointerUpOnDeckName()
    {
        editIcon.DOFade(1f, 0.3f).SetEase(Ease.OutExpo);
    }

    private async Task OnConfirmModifyDeckName(List<string> data)
    {
        var deckName = data[0];

        if (deckName.Length != 0)
        {
            await ModifyDeckName(deckName);
            return;
        }
        
        DOVirtual.DelayedCall(0.2f, () =>
        {
            var pop = popUpsInjector
                .Create<AdvancedPopUps>("pop_hint")
                .AppendText("deck_name_cannot_be_empty");
            if (pop is AdvancedPopUps adv)
                adv.cancelButton.gameObject.SetActive(false);
            pop.Display();
        });
    }
    
    private async Task ModifyDeckName(string deckName)
    {
        deckNameText.text = deckName;
        var rect = deckNameText.transform.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        
        if (_data == null)
            return;
        
        await NakamaManager.Instance.RenameDeck(_data.UniqueId, deckName);
    }
    
    #endregion

    #region Operation

    private async Task WriteCurrentDeckData(DeckData deck)
    {
        await NakamaManager.Instance.SaveDeck(deck);
        _deckList.Room.RefreshSelfCharacters();
    }
    
    private async Task SaveDeck()
    {
        if (CheckSaveFeasibility(out var deck))
        {
            await WriteCurrentDeckData(deck);
            DisplayHint("deck_saved");
        }
        else
            CreateUnsavedDeckPopUps(
                "deck_not_meet_requirements",
                "this_deck_not_meet_requirements",
                deck
            );
    }

    private void ExitBuildMode()
    {
        if (CheckSaveFeasibility(out var deck))
            _deckList.sceneLoader.UnloadCurrentScene();
        else
            CreateUnsavedDeckPopUps(
                "exit_editor",
                "change_and_exit_or_not",
                deck,
                () => _deckList.sceneLoader.UnloadCurrentScene()
            );
    }
    
    private void ClearCurrentDeck()
    {
        chosenCardArea.Clear();
        chosenCharacterArea.Clear();
    }

    private void CreateUnsavedDeckPopUps(
        string titleEntry, string content, 
        DeckData data, Action onCancel = null
    )
    {
        var pop = popUpsInjector
            .Create<AdvancedPopUps>(titleEntry)
            .SetEntry("save_changes", "discard_changes")
            .SetAsyncCallbacks(async _ =>
            {
                await WriteCurrentDeckData(data);
                _deckList.sceneLoader.UnloadCurrentScene();
            })
            .SetCallbacks(null, onCancel);

        pop.AppendText(content);
        if (!data.CheckCharacterCards())
            pop.AppendText("deck_must_contain_3_char");
        if (!data.CheckActionCards())
            pop.AppendText("deck_must_contain_30_action");
        if (!data.CheckInvalidActionCards())
            pop.AppendText("deck_contain_unplayed_card");
        
        pop.Display();
    }

    private bool CheckSaveFeasibility(out DeckData deck)
    {
        deck = _data ?? new DeckData
        {
            UniqueId = Guid.NewGuid(),
            isUsing = false,
            basePreset = _preset
        };
        
        deck.deckName = deckNameText.text;
        deck.characters = chosenCharacterArea.ToList();
        deck.actionCards = chosenCardArea.ToList();
        
        return deck.CheckSaveFeasibility();
    }
    
    #endregion

    private void LoadDeckData()
    {
        // Initialize the inventory card instance
        var maxInventoryInstances = Mathf.Max(
            characterTypes.Select(property => FilterCardByProperty(property).Count).Max(),
            actionCardTypes.Select(property => FilterCardByProperty(property).Count).Max()
        );
        inventoryArea.Initialize(maxInventoryInstances);
        
        if (_data == null)
            return;
        
        _data.actionCards.ForEach(chosenCardArea.Append);
        for (var i = 0; i < 3; i++)
            chosenCharacterArea.Append(_data.characters[i], i);
    }
    
    private void InitializeExtendFilterItems(List<Property> properties)
    {
        var list = extendType.children;
        for (var i = 0; i < list.Count; i++)
            list[i].Initialize(properties[i].ToSnakeCase());
        
        list.First().Choose();
    }
    
    private void OnBaseTypeChanged(string key)
    {
        var isCharacter = key == "Character";
        
        chosenCardArea.gameObject.SetActive(!isCharacter);
        chosenCharacterArea.gameObject.SetActive(isCharacter);
        
        var types = isCharacter ? characterTypes : actionCardTypes;
        if (!isCharacter)
        {
            var characters = chosenCharacterArea.ToList();
            var cards = chosenCardArea.Cards
                .Select(wrapper => wrapper.Card.Asset.CheckValidity(characters))
                .Where(asset => !asset.isValid)
                .ToList();
            cards.ForEach(chosenCardArea.Remove);
            
            if (cards.Count != 0)
                DisplayHint("return_invalid_chosen_deck");
        }
        
        InitializeExtendFilterItems(types);
    }

    private void OnExtendTypeChanged(string key)
    {
        var propStr = key.Replace("_", "");
        var property = Enum.Parse<Property>(propStr);
        var cards = FilterCardByProperty(property);
        
        inventoryArea.ReplaceCardItem(cards);
    }

    private List<ICardAsset> FilterCardByProperty(Property property)
    {
        if (property == Property.All)
            return _pool.characterCards.OfType<ICardAsset>().ToList();
        
        if (characterTypes.Contains(property))
            return Filter(_pool.characterCards.OfType<ICardAsset>().ToList(), property);
     
        if (property != Property.CardAction)
            return Filter(_pool.actionCards.OfType<ICardAsset>().ToList(), property);
        
        // CardAction means all event card
        return _pool.actionCards
            .Where(card => card.cardType == ActionCardType.Event)
            .OfType<ICardAsset>()
            .ToList();
        
        List<ICardAsset> Filter(List<ICardAsset> list, Property p)
        {
            return list.Where(card => card.Properties.Contains(p)).ToList();
        }
    }
}