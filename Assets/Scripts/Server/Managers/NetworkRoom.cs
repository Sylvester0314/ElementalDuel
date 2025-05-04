using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Client.UI.Misc.Transition;
using Shared.Handler;
using Shared.Classes;
using Client.Logic.Response;
using Shared.Misc;
using Unity.Netcode;
using String = Shared.Misc.String;

namespace Server.Managers
{
    public class NetworkRoom : NetworkBehaviour
    {
        public NetworkObject networkObject;
        public NetworkVariable<String> id = new();
        public NetworkVariable<RoomConfiguration> config = new();
        public NetworkVariable<bool> playerReadyStatus = new();
        public NetworkVariable<PlayerInformation> owner = new();
        
        public List<PlayerInformation> Players { get; private set; }
        public GameManager GameManager;

        private const int MaxPlayers = 2;
        private Global _game;

        #region Initialize Methods

        public void Start()
        {
            playerReadyStatus.OnValueChanged += (_, value) => OnReadyStatusChangedClientRpc(value);
        }

        public NetworkRoom Initialize(string roomId, RoomConfiguration configuration)
        {
            networkObject.Spawn();

            Players = new List<PlayerInformation>();
            id.Value = new String(roomId);
            config.Value = configuration;
            playerReadyStatus.Value = false;

            return this;
        }

        #endregion

        #region Lobby Methods

        public bool AddPlayer(ulong playerId, string loginId)
        {
            if (Players.Count >= MaxPlayers)
                return false;

            var newPlayer = new PlayerInformation
            {
                clientId = playerId,
                nakamaId = loginId
            };

            if (Players.Count == 0)
                owner.Value = newPlayer;

            Players.Add(newPlayer);
            networkObject.NetworkShow(playerId);

            return true;
        }

        public bool RemovePlayer(ulong playerId, bool isDisconnect = false)
        {
            Players.RemoveAll(player => player.clientId == playerId);
            if (!isDisconnect)
                networkObject.NetworkHide(playerId);

            if (Players.Count == 0)
            {
                Destroy(gameObject);
                return false;
            }

            // If a member left this room, and there are still player in 
            // this room, make this member the room owner
            if (owner.Value.clientId == playerId)
                owner.Value = Players.First();

            // When a new owner gene appears, reset the room's ready status
            // and call the new owner's client
            playerReadyStatus.Value = false;
            var remainId = Players.First().clientId;
            PlayerLeftForRoomClientRpc(NetCodeMisc.RpcParamsWrapper(remainId));

            return true;
        }

        public RoomInformation GetInformation()
        {
            return new RoomInformation
            {
                roomId = id.Value.content,
                ownerUid = owner.Value.nakamaId,
                playerCount = Players.Count,
                BaseConfigs = new List<RoomTagPair>
                {
                    new("CardPoolPreset", config.Value.cardPoolPreset),
                    new("GameMode", config.Value.gameMode)
                },
                Options = new List<RoomTagPair>
                {
                    new("DiceMode", config.Value.diceMode),
                    new("ContemplationTime", config.Value.contemplationTime)
                }
            };
        }

        #endregion

        #region Client Runtime Env

        public override void OnNetworkSpawn()
        {
            if (!IsClient)
                return;

            DontDestroyOnLoad(gameObject);
            StartCoroutine(WaitForPlayerReady());
        }

        private IEnumerator WaitForPlayerReady()
        {
            PrepareRoom room = null;
            while (ReferenceEquals(room, null))
            {
                room = FindObjectOfType<PrepareRoom>();
                yield return null;
            }

            room.Initialize();
        }

        [ClientRpc]
        public void PlayerJoinedClientRpc(bool isCreator, ClientRpcParams rpcParams = default)
        {
            IRoomJoinedCallbackHandler page = isCreator
                ? FindObjectOfType<CreateRoomPage>()
                : FindObjectOfType<LobbyPage>();

            page.RoomJoinedCallback();
        }

        [ClientRpc]
        public void NotifyOwnerNewPlayerClientRpc(string newPlayerId, ClientRpcParams rpcParams = default)
        {
            var room = FindObjectOfType<PrepareRoom>();

            // When a new player join this room, update the opponent
            // information display
            room.SetOpponentInformationContent(newPlayerId);
        }

        [ClientRpc]
        private void PlayerLeftForRoomClientRpc(ClientRpcParams _ = default)
        {
            var room = FindObjectOfType<PrepareRoom>();
            room.SetInformationContent(true);
        }

        [ClientRpc]
        private void OnReadyStatusChangedClientRpc(bool status)
        {
            var room = FindObjectOfType<PrepareRoom>();

            // If this client is not owner, this show the rpc comes
            // from self ready action, otherwise comes from the other
            // room member's ready action
            var isOwner = room.IsOwner();
            var infoDisplay = isOwner ? room.oppoDisplay : room.selfDisplay;

            // Modify the room member's (not owner) namebar content
            infoDisplay.SetReadyStyle(isOwner, status);
        }

        #endregion

        #region Game Manager Rpc

        [ClientRpc]
        public void GameLoadingClientRpc(string data)   
        {
            var room = FindObjectOfType<PrepareRoom>();
            
            room.sceneLoader.LoadScene(
                "GameScene",
                lazyLoad: true,
                transition: FixedScene.Instance.room,
                beforeFadeIn: transition => transition.Initialize(data),
                onSceneLoaded: _ => room.GameSceneLoadingComplete()
            );
        }
        
        [ClientRpc]
        public void GameLoadingAllCompleteClientRpc()
        {
            // Close transition page and start game
            FindObjectOfType<PrepareRoom>().sceneLoader.ActiveFlag();
        }

        [ClientRpc]
        public void ResponseClientRpc(ActionResponseWrapper[] wrappers, string uniqueId = null, ClientRpcParams _ = default)
        {
            // Only used in client
            if (_game == null)
                _game = FindObjectOfType<Global>();

            uniqueId ??= Guid.Empty.ToString();
            _game.Receive(wrappers, uniqueId);
        }

        #endregion
    }
}