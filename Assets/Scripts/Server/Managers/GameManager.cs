using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Managers;
using Client.UI.Misc.Transition;
using Server.GameLogic;
using Shared.Classes;
using Client.Logic.Response;
using Client.Logic.Request;
using Server.ResolveLogic;
using Shared.Misc;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Server.Managers
{
    public class GameManager
    {
        public Xoshiro Random;
        public RoomConfiguration Configuration;

        public readonly NetworkRoom Room;
        public readonly Dictionary<ulong, int> MappingId;
        public readonly Dictionary<int, ulong> InverseId;
        public readonly ReactionLogic ReactionLogic;
        public readonly TurnManager TurnManager;
        public readonly ActionRequestReceiver Receiver;
        public readonly ResolveManager ResolveManager;
        public readonly List<PlayerLogic> Logics;
        
        [Header("Process Data")]
        public int CurrentRound;
        public Dictionary<string, ResolveNode> BranchesTemp;
        public Dictionary<string, int> SynchronousCounts;
        public ActionResponseWrapper[][] SynchronousTemp;
        
        #region Initialize

        public GameManager(NetworkRoom room)
        {
            Room = room;
            MappingId = new Dictionary<ulong, int>();
            InverseId = new Dictionary<int, ulong>();
            ReactionLogic = new ReactionLogic();
            Logics = new List<PlayerLogic>();
            
            Random = Xoshiro.Create();
            Configuration = Room.config.Value;
            SynchronousTemp = new ActionResponseWrapper[2][];
            SynchronousCounts = new Dictionary<string, int>();
            
            var ids = Room.Players
                .Select(information => information.clientId)
                .ToList();

            for (var i = 0; i < 2; i++)
            {
                MappingId.Add(ids[i], i);
                InverseId.Add(i, ids[i]);
            }
            
            Receiver = new ActionRequestReceiver(this);
            TurnManager = new TurnManager(this);
            ResolveManager = new ResolveManager(this);
            BranchesTemp = new Dictionary<string, ResolveNode>();
        }

        public async void Initialize()
        {
            var dictionary = NetworkManager.Singleton.ConnectedClients;

            // Get players' active deck
            var tasks = MappingId.Keys
                .Select(id =>
                {
                    var instance = dictionary[id].PlayerObject;
                    var player = instance.GetComponent<PlayerManager>();
                    var rawDeck = player.activeDeck.Value;

                    return rawDeck.Parse();
                })
                .ToList();

            var decks = await Task.WhenAll(tasks);
            var initialData = new List<RoomTransitionInformation>();
            
            // Create player game data logic instance and
            // generate the initial information for players to display
            for (var i = 0; i < 2; i++)
            {
                var deck = decks[i];
                var logic = new PlayerLogic(this, InverseId[i], deck);
                var characters = logic.CharacterLogic.Characters;

                var information = new RoomTransitionInformation
                {
                    owner = Room.Players[i],
                    characters = deck.characters.Select(asset => asset.name).ToList(),
                    actionCardsCount = deck.actionCards.Count,
                    uniqueIds = characters.Select(data => data.UniqueId).ToList(),
                    statusIds = characters.Select(data => data.Statuses.UniqueId).ToList(),
                    zoneIds = new List<string>
                    {
                        logic.CombatStatuses.UniqueId,
                        logic.SummonZone.UniqueId,
                        logic.SupportZone.UniqueId
                    }
                };
                
                Logics.Add(logic);
                initialData.Add(information);
            }
            
            for (var i = 0; i < 2; i++)
                Logics[i].SetOpponent(Logics[i ^ 1]);
            
            var wrapper = new NetworkListWrapper<RoomTransitionInformation>(initialData);

            // Callback and open the transition page
            Room.GameLoadingClientRpc(JsonUtility.ToJson(wrapper));
        }

        #endregion

        #region Sandbox
        
        public static GameManager Sandbox(NetworkRoom room)
        {
            room.Players.Add(new PlayerInformation
            {
                clientId = 10000UL,
                nakamaId = ""
            });

            return new GameManager(room);
        }

        public async void VirtualInitialize()
        {
            var actionCards = new List<string>
            {"312001","312002","312003","312004","312005","312006","312101","312102","312201","312202","312301","312302","312302","312401","312402","312501","312502","312601","312602","312701","312702","321001","321002","321004","321005","321006","322003","322004","322005","322006"
            };
            var characterCards = new List<string> { "1601", "1201", "1101" };

            var rawDeckData = new RawDeckData
            {
                uniqueId = "00000000-0000-0000-0000-000000000000",
                isUsing = true,
                deckName = "",
                basePreset = "weird",
                characterCards = characterCards.ToList(),
                actionCards = actionCards.ToList()
            };
            var deckData = await rawDeckData.Parse();
            var decks = new [] { deckData, deckData };

            for (var i = 0; i < 2; i++)
                Logics.Add(new PlayerLogic(this, InverseId[i], decks[i]));
            for (var i = 0; i < 2; i++)
                Logics[i].SetOpponent(Logics[i ^ 1]);
        }

        #endregion

        #region Network

        public async void Receive(ActionRequestWrapper wrapper)
        {
            if (wrapper.Unblock)
                await wrapper.Request.Process(this);
            else
                Receiver.Enqueue(wrapper);
        }
        
        #endregion

        #region Process
        
        public void ControlSynchronousOperation(string key, Action both, Action half = null)
        {
            SynchronousCounts[key] = SynchronousCounts.GetValueOrDefault(key, 0) + 1;

            if (SynchronousCounts[key] != 2)
                half?.Invoke();
            else
            {
                SynchronousCounts[key] = 0;
                both.Invoke();   
            }
        }

        public void SwitchStartingHand()
        {
            var wrappers = new ActionResponseWrapper[2][];
            foreach (var (playerId, i) in MappingId)
            {
                var deck = Logics[i].DeckCard;
                var (startingHandList, _) = deck.Draw(5);
                
                var (t1, _) = deck.DrawBySpecificName(1, "322001");
                var (t2, _) = deck.DrawBySpecificName(2, "333006");
                startingHandList.AddRange(t1);
                startingHandList.AddRange(t2);
                
                deck.Append(startingHandList);
                
                var drawResponse = DrawResponse.Starting(playerId, startingHandList);
                var switchResponse = SwitchCardResponse.Starting(playerId);
                var responses = new IActionResponse[] { drawResponse, switchResponse };

                wrappers[i] = ActionResponseWrapper.Package(responses);
            }

            foreach (var (playerId, i) in MappingId)
            {
                var unionWrapper = wrappers[i].Concat(wrappers[i ^ 1]).ToArray();
                var rpcParams = NetCodeMisc.RpcParamsWrapper(playerId);
                Room.ResponseClientRpc(unionWrapper, _: rpcParams);
            }
        }

        public void StartNewRound()
        {
            CurrentRound += 1;

            foreach (var (playerId, i) in MappingId)
            {
                TurnManager.SetActingPlayer();

                var logic = Logics[i];
                var roundsResponse = PromptResponse.Round(CurrentRound, playerId == TurnManager.FirstId);
                var bannerResponse = PromptResponse.Phase("roll");

                foreach (var character in logic.CharacterLogic.Characters)
                    character.Skills.Values.ForEach(skill => skill.Variables.Set("RoundUsedCount", 0));

                // TODO 时点实现 - 投掷阶段 -> 重投次数
                var times = 1; 
                var dices = logic.Resource.Roll();
                var rerollResponse = new RerollResponse(playerId, dices, times);
                
                var responses = new IActionResponse[] { roundsResponse, bannerResponse, rerollResponse };
                var rpcParams = NetCodeMisc.RpcParamsWrapper(playerId);
                
                Room.ResponseClientRpc(ActionResponseWrapper.Package(responses), _: rpcParams);
            }
        }

        public void StartActionPhase()
        {
            var firstPlayer = TurnManager.FirstActive;

            var wrappers = ActionResponseWrapper.Union(
                new IActionResponse[]
                {
                    PromptResponse.Phase("action"),
                    PromptResponse.Action(firstPlayer.Id, false)
                },
                ResolveManager.PassiveTrigger(Timing.OnActionPhaseBeginning, firstPlayer)
            );

            RefreshCostAndResolutionCaches();
            Room.ResponseClientRpc(wrappers);
        }
        
        public void RefreshCostAndResolutionCaches()
        {
            foreach (var logic in Logics)
            {
                var rpcParams = NetCodeMisc.RpcParamsWrapper(logic.Id);
                var response = new UpdateCostsResponse(logic);
                var updateData = ActionResponseWrapper.Package(response);

                Room.ResponseClientRpc(updateData, _: rpcParams);
            }

            ResolveManager.CleanCache();
        }

        #endregion

        #region Misc

        public void SaveResponseTemporary(ulong requesterId, ActionResponseWrapper[] wrappers)
        {
            var id = MappingId[requesterId];
            SynchronousTemp[id] = wrappers;
        }
        
        public PlayerLogic RequesterLogic(ulong requesterId)
            => Logics[MappingId[requesterId]];

        public static PlayerLogic ClonePlayerState(PlayerLogic player)
        {
            var self = player.Clone();
            var oppo = player.Opponent.Clone();
            
            self.SetOpponent(oppo);
            oppo.SetOpponent(self);

            return self;
        }
        
        #endregion
    }
}