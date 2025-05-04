using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace Client.Managers
{
    public class ClientNetworkManager : NetworkManager
    {
        private Tween _tween;
        private const float ConnectTimeout = 5f;
        
        public void Awake()
        {
            OnClientStarted += () =>
            {
                _tween = DOVirtual.DelayedCall(ConnectTimeout, OnConnectTimeout);
            };

            OnConnectionEvent += (_, eventData) =>
            {
                if (eventData.EventType == ConnectionEvent.ClientConnected)
                    OnClientConnected();
                if (eventData.EventType == ConnectionEvent.ClientDisconnected)
                    OnClientDisconnected();
            };
        }

        private void OnClientConnected()
        {
            PlayerPrefs.SetInt("HandbookContainer", 1);
            PlayerPrefs.Save();
            
            _tween?.Kill();
            Handbook.Instance.SwitchContainerDisplay(1);
        }
        
        private void OnClientDisconnected()
        {
            PlayerPrefs.SetInt("HandbookContainer", 0);
            PlayerPrefs.Save();

            NakamaManager.Instance.LogoutAccount();
            Handbook.Instance.SwitchContainerDisplay(0);
        }

        private void OnConnectTimeout()
        {
            Singleton.Shutdown();
            Handbook.Instance.popUps
                .Create<PopUps>("pop_hint")
                .AppendText("network_connection_exception")
                .Display();
        }
    }
}