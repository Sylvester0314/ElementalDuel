using System.Collections.Generic;
using System.Linq;
using Client.Managers;
using Client.UI.Misc.Transition;
using Shared.Enums;
using Client.Logic.Response;
using DG.Tweening;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;

public class Global : MonoBehaviour
{
    public SceneLoader sceneLoader;
    
    [Header("UI Components")]
    public HandCards hand;
    public OpponentHand oppoHand;
    public CardBuffer buffer;
    public PromptUI prompt;
    public CombatActionUI combatAction;
    public TurnIndicator indicator;
    public InformationUI information;
    public DiceFunction diceFunction;
    public ShootEffectContainer shoot;
    public DiceReroll reroll;
    public List<Player> players;

    [Header("In Game Data")] 
    public bool acting;
    public bool startingPhase;
    public bool isPlayerAreaDisplay;
    public bool isTurnInformationDisplay;
    public bool attackAnimating;
    public AbstractCard selectingCard;
    public CharacterCard previewingMainTarget;
    public CharacterCard switchActiveTarget;
    public Dictionary<string, IGameEntity> EntitiesMap;
    public Dictionary<string, IStatusesContainer> StatusContainers;
    public Dictionary<string, ResolveOverview> Overviews;

    [HideInInspector] 
    public PlayerManager manager;
    [HideInInspector] 
    public PrepareRoom prepareRoom;
    public IActionResponse CurrentResponse;
    public IActionResponse BlockingResponse;

    public Player Self => players[0];
    public Player Opponent => players[1];

    public bool Acting
    {
        get => acting;
        set {
            acting = value; 
            
            Self.SetActiveStatus(value);
            Opponent.SetActiveStatus(!value);
            
            indicator.Switch(value);
        }
    }
    
    public void Initialize(List<GamePlayerInformation> initialData)
    {
        prepareRoom = FindObjectOfType<PrepareRoom>();
        
        combatAction.Initialize();
        EntitiesMap = new Dictionary<string, IGameEntity>();
        StatusContainers = new Dictionary<string, IStatusesContainer>();
        Overviews = new Dictionary<string, ResolveOverview>();
        
        for (var i = 0; i < 2; i++)
            players[i].Initialize(initialData[i]);
        
        manager = NetworkManager.Singleton
            .LocalClient.PlayerObject
            .GetComponent<PlayerManager>();
    }

    public void BackToRoom()
    {
        prepareRoom.sceneLoader.UnloadCurrentScene();
    }

    #region Network

    public void Receive(ActionResponseWrapper[] wrappers, string uniqueId)
    {
        if (wrappers.Length == 0)
            return;

        var headResponse = ActionResponseWrapper.Chainable(this, wrappers, uniqueId);
        
        if (wrappers.First().Unblock)
            headResponse.Process();
        
        else if (CurrentResponse != null)
            CurrentResponse.Tail.NextResponse = headResponse;
        else
        {
            CurrentResponse = headResponse;
            CurrentResponse.Process();
        }
    }
    
    public void CreateVirtualEnvironment()
    {
        NetworkManager.Singleton.StartClient();
        DOVirtual.DelayedCall(0.5f, () =>
        {
            manager = NetworkManager.Singleton
                .LocalClient.PlayerObject
                .GetComponent<PlayerManager>();

            manager.CreateVirtualEnvironmentServerRpc();
            Debug.Log("虚拟环境创建成功");
        });
    }

    #endregion

    #region Game Process
    
    public void GameStart()
    {
        SetPlayerAreaStatus(true);
        GetZone<CharacterZone>(Site.Self).ChooseActiveCharacter();

        const string entry = "select_first_active_character";
        var selectHint = ResourceLoader.GetLocalizedUIText(entry);
        prompt.banner.Animate(selectHint, false, true);
    }

    public void OpenRerollScene(List<DiceLogic> dices, int times)
    {
        diceFunction.RerollDices = dices;
        diceFunction.rerollTimes = times;
        sceneLoader.LoadScene("RerollScene", lazyLoad: true);
    }
    
    #endregion

    #region Global Data Getter
    
    public T GetZone<T>(Site site) where T : AbstractZone
    {
        var i = (int)site;
        
        if (players[i].characterZone is T castedCharacterArea)
            return castedCharacterArea;
        if (players[i].deck is T castedDecArea)
            return castedDecArea;
        
        return null;
    }
    
    public CharacterCard GetCharacter(string uniqueId) 
        => GetEntity(uniqueId) as CharacterCard;
    
    public IGameEntity GetEntity(string uniqueId)
        => EntitiesMap.GetValueOrDefault(uniqueId);
    
    #endregion

    #region Components Controllor

    public void SetSelectingCard(AbstractCard card)
    {
        if (selectingCard == card)
        {
            if (ReferenceEquals(selectingCard, null))
                information.CloseAll();
            return;
        }
        
        if (selectingCard != null)
            selectingCard.CloseSelectIcon();
        
        selectingCard = card;
        if (selectingCard is CharacterCard character)
        {
            if (prompt.HasComponentShowing())
            {
                prompt.CloseAll();
                diceFunction.ResetLayout();
                hand.gameObject.SetActive(true);
            }
            
            var selfActive = GetZone<CharacterZone>(Site.Self).Active;
            var selfActiveKey = selfActive?.uniqueId;
            
            var isSelf = character.zone.owner.site == Site.Self;
            var isActive = character.uniqueId == selfActiveKey;
            if (!isSelf || isActive || character.currentHealth == 0)
                combatAction.TransferStatus(CombatTransfer.Active);
            else
            {
                combatAction.TransferStatus(CombatTransfer.Switch);
                switchActiveTarget = character;
            }
        }
        
        if (ReferenceEquals(selectingCard, null))
            information.CloseAll();
        else
        {
            information.Display(card);
            if (card is not PlayableActionCard)
                hand.ContractArea();
        }
    }

    public void SetPlayerAreaStatus(bool isDisplay)
    {
        isPlayerAreaDisplay = isDisplay;
        foreach (var player in players)
            player.SetStatus(isPlayerAreaDisplay);
    }

    public void SetTurnInformationStatus(bool isDisplay)
    {
        isTurnInformationDisplay = isDisplay;
        
        indicator.SetDisplayStatus(isTurnInformationDisplay);
        foreach (var player in players)
            player.nameBar.Fade(isTurnInformationDisplay, duration:0.15f);
    }

    #endregion

    #region Preview

    public void OpenPreviewUI(string first)
    {
        selectingCard?.CloseSelectIcon();
        switchActiveTarget = null;

        SetPreview(Overviews[first]);

        if (first == ResolveTree.Root)
            return;

        Overviews.Keys
            .Select(GetCharacter)
            .ToList()
            .ForEach(character => character.SwitchToSelectableStatus());
        
        // var keys = Overviews.Keys.ToList();
        // if (keys.Count == 1 && keys.First().Equals(ResolveTree.Root))
        //     return;
        //
        // keys.Select(GetCharacter)
        //     .ToList()
        //     .ForEach(character => character.SwitchToSelectableStatus());
        
        previewingMainTarget?.SelectCard();
    }

    public void SetPreview(ResolveOverview overview)
    {
        hand.rootCanvas.sortingOrder = 1;

        EntitiesMap.ForEach(pair => pair.Value.HidePreviewComponents());
        
        var curPreview = overview.Modifications.Keys.ToList();
        var prevPreview = EntitiesMap
            .Where(pair => pair.Value is CharacterCard { isPreviewing: true } &&
                           !curPreview.Contains(pair.Key) &&
                           !Overviews.Keys.Contains(pair.Key)
            )
            .Select(pair => pair.Value)
            .ToList();

        foreach (var (zoneId, modification) in overview.StatusModifications)
        {
            var container = StatusContainers[zoneId];
            if (container is not StatusCardZone cardZone)
                continue;
            
            cardZone.Preview(modification.Statuses);
        }
        
        foreach (var character in prevPreview)
            character.CancelPreviewStatus();
        
        foreach (var key in curPreview)
        {
            var character = GetCharacter(key);
            
            character.SwitchToPreviewingStatus(true);
            character.SetPreviewInformation(overview.Modifications[key]);
        }
    }

    public void SetPreview(string uniqueId)
    {
        if (Overviews.TryGetValue(uniqueId, out var overview))
            SetPreview(overview);
    }
    
    public void CancelPreview()
    {
        if (startingPhase || combatAction.choosing || attackAnimating)
            return;

        hand.rootCanvas.sortingOrder = 0;
        previewingMainTarget = null;
        EntitiesMap.Values
            .ToList()
            .ForEach(entity => entity.CancelPreviewStatus());
        StatusContainers
            .ForEach(pair => pair.Value.CancelPreview());
    }
    
    #endregion
}