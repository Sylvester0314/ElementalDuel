using System;
using System.Reflection;
using Client.Logic.Request;
using DG.Tweening;
using Server.Managers;
using Shared.Classes;
using Unity.Netcode;

namespace Client.Managers
{
    public class PlayerManager : NetworkBehaviour
    {
        public NetworkVariable<RawDeckData> activeDeck = new (
            new RawDeckData(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        
        private ServerGameManager _server;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
                DontDestroyOnLoad(gameObject);
        }
        
        private ServerGameManager CallServer()
        {
            _server ??= FindObjectOfType<ServerGameManager>();
            return _server;
        }
        
        #region Room Request Server Rpc
        
        [ServerRpc]
        public void CreateRoomServerRpc(RoomConfiguration config, string loginId, ServerRpcParams rpcParams = default)
        {
            var creatorId = rpcParams.Receive.SenderClientId;
            CallServer().CreateRoom(config, creatorId, loginId);
        }

        [ServerRpc]
        public void JoinRoomServerRpc(string roomId, string loginId, ServerRpcParams rpcParams = default)
        {
            var requesterId = rpcParams.Receive.SenderClientId;
            CallServer().JoinRoom(roomId, requesterId, loginId);
        }

        [ServerRpc]
        public void LeaveRoomServerRpc(ServerRpcParams rpcParams = default)
        {
            var requesterId = rpcParams.Receive.SenderClientId;
            CallServer().LeaveRoom(requesterId);
        }

        [ServerRpc]
        public void DisplayRoomInformationServerRpc(ServerRpcParams rpcParams = default)
        {
            var requesterId = rpcParams.Receive.SenderClientId;
            CallServer().DisplayRoomInformation(requesterId);
        }

        [ServerRpc]
        public void RoomPlayerReadyServerRpc(string roomId)
        {
            if (!CallServer().Rooms.TryGetValue(roomId, out var room))
                return;
            
            room.playerReadyStatus.Value = !room.playerReadyStatus.Value;
        }
        
        [ServerRpc]
        public void RoomGameStartServerRpc(string roomId)
        {
            if (!CallServer().Rooms.TryGetValue(roomId, out var room))
                return;

            room.GameManager = new GameManager(room);
            room.GameManager.Initialize();
        }
        
        #endregion

        #region Game Server Rpc

        [ServerRpc]
        public void CreateVirtualEnvironmentServerRpc(ServerRpcParams rpcParams = default)
        {
            var creatorId = rpcParams.Receive.SenderClientId;
            var server = CallServer();
            
            server.CreateRoom(
                new RoomConfiguration
                {
                    diceMode = "",
                    gameMode = "",
                    cardPoolPreset = "",
                    contemplationTime = ""
                }, creatorId, ""
            );
            
            var roomId = server.InGamePlayers[creatorId];
            if (!server.Rooms.TryGetValue(roomId, out var room))
                return;
            
            room.GameManager = GameManager.Sandbox(room);
            room.GameManager.VirtualInitialize();
        }
        
        [ServerRpc]
        public void GameSceneLoadingCompleteServerRpc(string roomId)
        {
            if (!CallServer().Rooms.TryGetValue(roomId, out var room))
                return;

            var manager = room.GameManager;
            manager.ControlSynchronousOperation("game_loading", () =>
            {
                // All players loading completely
                room.GameLoadingAllCompleteClientRpc();
            
                // Game Process Controller - Starting Hand
                DOVirtual.DelayedCall(2.5f, manager.SwitchStartingHand);
            });
        }

        [ServerRpc]
        public void SynchronousServerRpc(string key, string source, ServerRpcParams rpcParams = default)
        {
            var server = CallServer();
            
            var requesterId = rpcParams.Receive.SenderClientId;
            var roomId = server.InGamePlayers[requesterId];
            if (!server.Rooms.TryGetValue(roomId, out var room))
                return;

            var sourceClass = Type.GetType(source);
            if (sourceClass == null)
                return;
            
            var both = sourceClass.GetMethod("BothComplete", BindingFlags.Public | BindingFlags.Static);
            var half = sourceClass.GetMethod("HalfComplete", BindingFlags.Public | BindingFlags.Static);
            room.GameManager.ControlSynchronousOperation(key, Converter(both), Converter(half));
            
            return;

            Action Converter(MethodInfo method)
            {
                if (method == null)
                    return () => { };
                var parameters = method.GetParameters();
                if (parameters.Length == 2 &&
                    parameters[0].ParameterType == typeof(NetworkRoom) &&
                    parameters[1].ParameterType == typeof(ulong))
                    return () => method.Invoke(null, new object[] { room, requesterId });

                return () => { };
            }
        }

        [ServerRpc]
        public void RequestServerRpc(ActionRequestWrapper request)
        {
            var server = CallServer();
            var roomId = server.InGamePlayers[request.Request.RequesterId];
            
            if (!server.Rooms.TryGetValue(roomId, out var room))
                return;
            
            room.GameManager.Receive(request);
        }
        
        #endregion
    }
}