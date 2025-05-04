using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Client.Managers;
using Shared.Handler;
using Shared.Classes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateRoomPage : AbstractSettingPage, IRoomJoinedCallbackHandler
{
    [Header("Self Components")]
    public LobbyContentContainer parent;
    
    [Header("Create Room Buttons")]
    public Transform loading;

    private TweenerCore<Quaternion, Vector3, QuaternionOptions> _loop;
    
    public void Awake()
    {
        settingLogic = new SettingLogic(settingsContainer, scroll);

        parent.logic = settingLogic;
        button.Callback = CreateRoom;
    }

    public override void Open()
    {
        base.Open();
        button.textEvent.SetEntry("create_room");
        settingLogic.disableClick = false;

        _loop?.Kill();
        loading.gameObject.SetActive(false);
    }

    private void CreateRoom()
    {
        button.textEvent.SetEntry("creating_room");
        settingLogic.disableClick = true;
        
        loading.gameObject.SetActive(true);
        _loop = loading
            .DORotate(new Vector3(0, 0, -360), 2f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
        
        var config = settingLogic.ToData<RoomConfiguration>();
        var loginId = NakamaManager.Instance.Session.UserId;

        NetworkManager.Singleton.LocalClient
            .PlayerObject.GetComponent<PlayerManager>()
            .CreateRoomServerRpc(config, loginId);
    }

    public void RoomJoinedCallback()
    {
        DOVirtual.DelayedCall(0.1f, () =>
        {
            _loop?.Kill();
            loading.gameObject.SetActive(false);
            Handbook.Instance.sceneLoader.LoadScene(
                "RoomScene", LoadSceneMode.Single,
                lazyLoad: true
            );
        });
    }
}