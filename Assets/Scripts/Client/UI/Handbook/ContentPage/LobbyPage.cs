using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Managers;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Shared.Handler;
using Nakama.TinyJson;
using Shared.Classes;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyPage : AbstractBookPage, IRoomJoinedCallbackHandler
{
    [Header("Self Components")]
    public LobbyContentContainer parent;
    
    [Header("Room List Components")]
    public Transform roomListContent;
    public TMP_InputField roomIdInput;
    public GameObject emptyHint;
    public Transform loadingImage;
    public MiddleButton refreshButton;
    
    [Header("User Information Components")]
    public Image rankImage;
    public TextMeshProUGUI extraLevel;
    
    [Header("Prefab References")]
    public RoomItem roomItemPrefab;
    
    private PlayerManager _manager;
    private float _requestStartTime;
    private readonly List<RoomItem> _roomItems = new ();
    private TweenerCore<Quaternion, Vector3, QuaternionOptions> _loop;

    private const float ThrottlingTime = 0.2f;
    
    public void Start()
    {
        _manager = NetworkManager.Singleton
            .LocalClient.PlayerObject
            .GetComponent<PlayerManager>();
        refreshButton.Callback = () =>
        {
            refreshButton.body.transform
                .DORotate(new Vector3(0, 0, 180), 0.4f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear);

            RequestRenderRoomInfo();
        };

        RequestRenderRoomInfo();
    }

    public void RequestRenderRoomInfo()
    {
        if (Time.time - _requestStartTime < ThrottlingTime)
            return;
        _requestStartTime = Time.time;
        
        emptyHint.SetActive(false);
        roomListContent.gameObject.SetActive(false);
        roomIdInput.text = string.Empty;
        
        loadingImage.gameObject.SetActive(true);
        _loop = loadingImage
            .DORotate(new Vector3(0, 0, -360), 2f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
        
        _roomItems.ForEach(room => Destroy(room.gameObject));
        _roomItems.Clear();
        
        _manager.DisplayRoomInformationServerRpc();
    }

    public void SearchRoomId(string search)
    {
        var searchCount = 0;
        _roomItems.ForEach(room =>
        {
            var isContains = room.id.Contains(search);
            room.gameObject.SetActive(isContains);
            searchCount += isContains ? 1 : 0;
        });
        
        SetRoomListAreaContent(searchCount);
    }

    public async void DisplayRoomList(string jsonData)
    {
        var informationList = jsonData.FromJson<List<RoomInformation>>();
        
        foreach (var information in informationList)
        {
            var owner = await NakamaManager.Instance.GetPlayerData(information.ownerUid);
            if (owner == null) 
                continue;
            
            var room = Instantiate(roomItemPrefab, roomListContent, false);
            room.Initialize(information, owner, JoinTargetRoom);
            _roomItems.Add(room);
        }
        
        var duration = Time.time - _requestStartTime;
        var time = 1000 * (ThrottlingTime - duration);
        if (time > 0)
            await Task.Delay(Mathf.FloorToInt(time));
        
        _loop.Kill();
        loadingImage.gameObject.SetActive(false);
        SetRoomListAreaContent(_roomItems.Count);
    }

    public void JoinTargetRoom(string roomId)
    {
        var loginId = NakamaManager.Instance.Session.UserId;
        _manager.JoinRoomServerRpc(roomId, loginId);
    }

    public void RoomJoinedCallback()
    {
        Handbook.Instance.sceneLoader.LoadScene(
            "RoomScene", LoadSceneMode.Single,
            lazyLoad: true
        );
    }

    private void SetRoomListAreaContent(int count)
    {
        var isEmpty = count == 0;
        emptyHint.SetActive(isEmpty);
        roomListContent.gameObject.SetActive(!isEmpty);
    }
}