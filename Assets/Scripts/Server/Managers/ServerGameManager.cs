using System.Collections.Generic;
using System.Linq;
using Nakama.TinyJson;
using Shared.Classes;
using Shared.Misc;
using Unity.Netcode;
using UnityEngine;

namespace Server.Managers
{
    public class ServerGameManager : Shared.Managers.GameManager
    {
        public Dictionary<string, NetworkRoom> Rooms = new();
        public Dictionary<ulong, string> InGamePlayers = new();
        
        [Header("Components")]
        public NetworkRoom roomPrefab;
        
        protected override void Initialize()
        {
            if (IsServer)
                Debug.Log("Initializing server...");
        }
        
        public void CreateRoom(RoomConfiguration config, ulong creatorId, string loginId)
        {
            string roomId;
            do
            {
                roomId = Random.Range(10000, 100000).ToString();
            } while (Rooms.Keys.Contains(roomId));
            
            var room = Instantiate(roomPrefab).Initialize(roomId, config);
            Rooms.Add(roomId, room);
            
            JoinRoom(roomId, creatorId, loginId, true);
            Debug.Log($"Creating room: creator: {creatorId} ...");
        }

        public void JoinRoom(string roomId, ulong playerId, string loginId, bool isCreator = false)
        {
            if (!Rooms.TryGetValue(roomId, out var room))
            {
                ShowPopUpsClientRpc(
                    "hint", 
                    "not_existing_room".SingleList(),
                    NetCodeMisc.RpcParamsWrapper(playerId)
                );
                return;
            }

            if (!room.AddPlayer(playerId, loginId))
            {
                ShowPopUpsClientRpc(
                    "hint", 
                    "room_full_now".SingleList(),
                    NetCodeMisc.RpcParamsWrapper(playerId)
                );
                return;
            }

            InGamePlayers.Add(playerId, roomId);
            room.PlayerJoinedClientRpc(isCreator, NetCodeMisc.RpcParamsWrapper(playerId));

            if (isCreator)
                return;
            
            var ownerId = room.owner.Value.clientId;
            room.NotifyOwnerNewPlayerClientRpc(loginId, NetCodeMisc.RpcParamsWrapper(ownerId));
        }

        public void LeaveRoom(ulong playerId, bool isDisconnect = false)
        {
            if (!InGamePlayers.TryGetValue(playerId, out var roomId))
            {
                if (!isDisconnect)
                    ShowPopUpsClientRpc(
                        "hint",
                        "cannot_leave_from_not_joined_room".SingleList(),
                        NetCodeMisc.RpcParamsWrapper(playerId)
                    );
                return;
            }

            if (!Rooms.TryGetValue(roomId, out var room))
            {
                if (!isDisconnect)
                    ShowPopUpsClientRpc(
                        "hint",
                        "not_existing_room".SingleList(),
                        NetCodeMisc.RpcParamsWrapper(playerId)
                    );
                return;
            }

            // After a member left, if there is no player in this room, destroy it
            if (!room.RemovePlayer(playerId, isDisconnect))
                Rooms.Remove(roomId);
            
            InGamePlayers.Remove(playerId);
        }

        public void DisplayRoomInformation(ulong playerId)
        {
            var information = Rooms.Values
                .Select(room => room.GetInformation())
                .ToList();
            var json = information.ToJson();

            RenderRoomsClientRpc(json, NetCodeMisc.RpcParamsWrapper(playerId));
        }

        [ClientRpc]
        private void ShowPopUpsClientRpc(string title, List<string> entries, ClientRpcParams _ = default)
        {
            var pop = Handbook.Instance.popUps.Create<PopUps>("pop_" + title);
            var type = title == "hint";
            var count = entries.Count;

            for (var i = 0; i < count; i++)
            {
                pop = type 
                    ? pop.AppendText(entries[i]) 
                    : pop.AppendError(entries[i]);
            }
            
            pop.Display();
        }

        [ClientRpc]
        private void RenderRoomsClientRpc(string jsonData, ClientRpcParams _ = default)
        {
            var page = FindObjectOfType<LobbyPage>();
            page.DisplayRoomList(jsonData);
        }
    }
}