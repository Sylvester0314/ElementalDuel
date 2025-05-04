using Unity.Netcode;

namespace Server.Managers
{
    public class ServerNetworkManager : NetworkManager
    {
        private ServerGameManager _manager;
        
        public void Awake()
        {
            _manager = FindObjectOfType<ServerGameManager>();
            
            OnClientConnectedCallback += OnClientConnected;
            OnClientDisconnectCallback += OnClientDisconnected;
        }

        public void Start()
        {
            StartServer();
        }
        
        private void OnClientConnected(ulong clientId)
        {
            SpawnManager.GetPlayerNetworkObject(clientId).NetworkShow(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _manager.LeaveRoom(clientId, true);
        }
    }
}