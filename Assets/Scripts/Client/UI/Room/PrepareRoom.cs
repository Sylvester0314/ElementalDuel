using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Managers;
using Client.UI.Misc.Transition;
using DG.Tweening;
using Server.Managers;
using Shared.Classes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareRoom : MonoBehaviour
{
    public List<DeckData> currentDecks;

    [Header("Global Components")]
    public SceneLoader sceneLoader;
    public PopUpsInjector popUps;
    
    [Header("Components")]
    public PlayerInformationDisplay selfDisplay;
    public PlayerInformationDisplay oppoDisplay;
    public MenuDropDownList menuDropList;
    public MiddleButton menuButton;

    private bool _gameStarted;
    private bool _isOwner;
    private PlayerManager _manager;
    private NetworkRoom _room;
    private float _lastModifyReadyStatusTime;
    private Tween _fakeLoadingTween;

    public void Update()
    {
        if (!Input.GetMouseButtonDown(0) || 
            !menuDropList.isOpening || 
            menuDropList.clickingSub ||
            menuButton.clicking
        )
            return;
        
        menuDropList.SetStatus(false);
    }

    public async Task UpdateDecks()
    {
        currentDecks = await NakamaManager.Instance.GetDecks();
        RefreshSelfCharacters();
    }
    
    #region Initialize Methods

    public void Awake()
    {
        _manager = NetworkManager.Singleton.LocalClient
            .PlayerObject.GetComponent<PlayerManager>();
    }

    public bool IsOwner()
    {
        var localId = NetworkManager.Singleton.LocalClientId;
        var ownerId = _room.owner.Value.clientId;
        return ownerId == localId;
    }

    public async void Initialize()
    {
        _room = FindObjectOfType<NetworkRoom>();
        
        menuDropList.SetSubButtonCallback("invite", InviteRoom);
        menuDropList.SetSubButtonCallback("setting", OpenSettings);
        menuDropList.SetSubButtonCallback("rules", OpenCheckRules);
        menuDropList.SetSubButtonCallback("exit", ExitRoom);

        menuButton.Callback = menuDropList.SwitchStatus;
        
        await UpdateDecks();
        SetInformationContent(IsOwner());
        StartCoroutine(SceneLoadOnComplete());
    }

    private IEnumerator SceneLoadOnComplete()
    {
        while (!selfDisplay.nameBar.roomFirstLoad)
            yield return null;

        FindObjectsByType<SceneLoader>(FindObjectsSortMode.InstanceID)
            .Where(el => el.key == "LobbyScene")
            .First()
            .ActiveFlag();
    }
    
    #endregion

    #region Ready Status Display

    public void SetInformationContent(bool isOwner)
    {
        _isOwner = isOwner;

        SetSelfInformationContent(_isOwner);

        if (_isOwner)
        {
            oppoDisplay.nameBar.SetNoPlayerStyle();
            return;
        }

        // if this client is not owner, set the opponent information display
        var ownerId = _room.owner.Value.nakamaId;
        SetOpponentInformationContent(ownerId);
    }

    public void SetSelfInformationContent(bool isOwner)
    {
        _isOwner = isOwner;
        var entry = _isOwner ? "start_game" : "preparing_for_game";
        Action action = _isOwner ? StartGame : PreparingForGame;

        selfDisplay.Initialize(
            NakamaManager.Instance.self, action, 
            entry, CardDeckBuilding
        );

        RefreshSelfCharacters();

        if (_isOwner)
            selfDisplay.SetReadyStyle(_isOwner, true);
    }

    public async void SetOpponentInformationContent(string playerId)
    {
        var playerData = await NakamaManager.Instance.GetPlayerData(playerId);
        oppoDisplay.Initialize(playerData);
        oppoDisplay.SetReadyStyle(_isOwner, !_isOwner);
    }

    public void RefreshSelfCharacters()
    {
        var active = selfDisplay.SetCharacters(currentDecks);
        _manager.activeDeck.Value = active.ToRaw();
    }

    #endregion

    #region Operation

    private void PreparingForGame()
    {
        if (Time.time - _lastModifyReadyStatusTime < 0.5f)
            return;

        var roomId = _room.id.Value.content;
        _manager.RoomPlayerReadyServerRpc(roomId);
        _lastModifyReadyStatusTime = Time.time;
    }

    private void StartGame()
    {
        if (!_room.playerReadyStatus.Value)
        {
            popUps.Create<PopUps>("pop_hint")
                .AppendText("have_player_not_ready_for_game")
                .Display();
            return;
        }

        if (_room.GameManager != null)
        {
            popUps.Create<PopUps>("pop_hint")
                .AppendText("game_already_started")
                .Display();
            return;
        }
        
        if (_gameStarted)
            return;

        _gameStarted = true;
        _manager.RoomGameStartServerRpc(_room.id.Value.content);
    }
    
    private void CardDeckBuilding()
    {
        if (!_isOwner && _room.playerReadyStatus.Value)
        {
            popUps.Create<PopUps>("pop_hint")
                .AppendText("cannot_change_when_preparing")
                .Display();
            return;
        }
        
        sceneLoader.LoadScene("DeckScene", lazyLoad: true);
    }

    private void OpenCheckRules()
    {
        Debug.Log("Opening check rules");
    }

    private void OpenSettings()
    {
        Debug.Log("Opening settings");
    }

    private void InviteRoom()
    {
        var pattern = ResourceLoader.GetLocalizedUIText("copy_buffer_pattern_room");

        var preset = _room.config.Value.cardPoolPreset;
        var roomId = _room.id.Value.content;
        var mode = ResourceLoader.GetLocalizedUIText($"rih_{preset}");
       
        var text = pattern.Replace("$[Room]", roomId).Replace("$[Mode]", mode);
        GUIUtility.systemCopyBuffer = text;
        
        popUps.Create<PopUps>("pop_hint")
            .AppendText("room_info_copied_to_clipboard")
            .Display();
    }
    
    private void ExitRoom()
    {
        _manager.LeaveRoomServerRpc();
        sceneLoader.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    public Task GameSceneLoadingComplete()
    {
        var roomId = _room.id.Value.content;
        _manager.GameSceneLoadingCompleteServerRpc(roomId);

        var game = FindObjectOfType<Global>();
        game.Initialize(FixedScene.Instance.room.Information);
        
        return Task.CompletedTask;
    }

    #endregion
}