using Shared.Enums;
using Client.UI.Misc.Transition;
using DG.Tweening;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Global global;
    public DisplayCard usingDisplay;
    
    [Header("Self Components")]
    public Deck deck;
    public CharacterZone characterZone;
    public StatusZone combatStatuses;
    public StatusCardZone summonZone;
    public StatusCardZone supportZone;
    public NameBar nameBar;

    [Header("References")] 
    public Sprite actingBar;
    public Sprite inactiveBar;

    [Header("In Game Data")] 
    public Site site;

    public void Awake()
    {
        characterZone.Initialize(this);
    }

    public void Initialize(GamePlayerInformation information)
    {
        var zoneIds = information.ZoneIds;
        var statusIds = information.StatusIds;
        
        for (var i = 0; i < 3; i++)
        {
            var character = characterZone.characters[i];
            var uniqueId = information.UniqueIds[i];
            
            character.uniqueId = uniqueId;
            character.statuses.Initialize(statusIds[i], global);
            character.statuses.belongs = character;
            character.LoadAsset(information.Assets[i]);
            
            global.EntitiesMap.Add(uniqueId, character);
        }

        combatStatuses.Initialize(zoneIds[0], global);
        summonZone.Initialize(zoneIds[1], global);
        supportZone.Initialize(zoneIds[2], global);
        
        nameBar.playerName.text = information.Name;
        nameBar.avatar.sprite = information.Avatar;
        nameBar.legend.Remaining = 1;
        deck.Initialize(this, information.ActinCardsCount);
    }

    public void SetStatus(bool status)
    {
        var alpha = status ? 1 : 0;
        deck.canClick = status;

        characterZone.canvas.DOFade(alpha, 0.15f).SetEase(Ease.OutExpo);
    }
    
    public void SetActiveStatus(bool status, bool isEnd = false)
    {
        var entry = isEnd 
            ? "namebar_status_end_phase" 
            : status 
                ? "namebar_status_now_acting"
                : "namebar_status_now_waiting";
        var sprite = status ? actingBar : inactiveBar;
        
        nameBar.statusEvent.SetEntry(entry);
        nameBar.background.sprite = sprite;
        nameBar.arrow.gameObject.SetActive(status);

        if (!status)
        {
            nameBar.ArrowTween?.Kill();
            return;
        }
        
        nameBar.ArrowTween = nameBar.arrow
            .DOLocalMoveX(-81.5f, 0.8f)
            .From(-85)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void SetEndPhaseStyle()
    {
        SetActiveStatus(false, true);
    }
}