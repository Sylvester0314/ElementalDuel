using System.Collections.Generic;
using System.Linq;
using Client.Logic.Response;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Enums;
using Shared.Logic.Effect;
using Shared.Misc;

namespace Server.Managers
{
    public class TurnManager
    {
        public GameManager Game;
        
        public ulong FirstId;
        public ulong ActingId;
        public int ActingRemainingActions;
        
        public List<ulong> DeclaredRoundEnds;

        public PlayerLogic FirstActive => Game.RequesterLogic(FirstId);
        public PlayerLogic Acting => Game.RequesterLogic(ActingId);
        public ResolveManager Resolve => Game.ResolveManager;

        private int _remainingDelta;

        public TurnManager(GameManager game)
        {
            Game = game;

            var index = Game.Random.NextInt(2);
            FirstId = Game.InverseId[index];
            DeclaredRoundEnds = new List<ulong>();
        }

        public void DoAction(bool isCombat)
        {
            _remainingDelta = isCombat ? 1 : 0;
            
            ActingRemainingActions -= _remainingDelta;
        }

        public void AfterAction(bool isEnd)
        {
            var response = isEnd ? DeclaredRoundEnd() : PerformedCombatAction();
            
            Game.RefreshCostAndResolutionCaches();
            
            if (_remainingDelta == 0 || response == null)
                return;
            
            var wrappers = ActionResponseWrapper.Package(response);
            Game.Room.ResponseClientRpc(wrappers);
        }

        private IActionResponse DeclaredRoundEnd()
        {
            if (DeclaredRoundEnds.Count == 0)
                return null;
            
            SwitchActing();
            
            return PromptResponse.Action(ActingId, false);
        }

        private IActionResponse PerformedCombatAction()
        {
            if (DeclaredRoundEnds.Count != 0)
                return PromptResponse.Action(ActingId, false, true);
            
            var isContinue = ActingRemainingActions > 0;
            if (isContinue == false)
                SwitchActing();
            
            return PromptResponse.Action(ActingId, false, isContinue);
        }

        public bool CheckActing(ulong requesterId)
        {
            return requesterId == ActingId && !DeclaredRoundEnds.Contains(requesterId);
        }

        public bool Check(ulong requesterId, string requestUnique)
        {
            if (CheckActing(requesterId))
                return true;
            
            var dialogResponse = PromptResponse.Dialog("warning_hint_turn");
            var rpcParams = NetCodeMisc.RpcParamsWrapper(requesterId);
            var wrappers = ActionResponseWrapper.Package(dialogResponse);
            
            Game.Room.ResponseClientRpc(wrappers, requestUnique, rpcParams);
            Game.Receiver.Dequeue(requestUnique);
            return false;
        }

        public void SwitchActing()
        {
            var actingIndex = Game.MappingId[ActingId];
            var nextActing = Game.InverseId[actingIndex ^ 1];
            
            SetActingPlayer(nextActing);
        }

        public void SetActingPlayer(ulong playerId = ulong.MaxValue)
        {
            ActingId = playerId == ulong.MaxValue ? FirstId : playerId;
            ActingRemainingActions = 1;
        }

        // public ActionResponseWrapper[][] DeclareEndRound(ulong requester)
        // {
        //     var wrappers = new ActionResponseWrapper[3][];
        //
        //     wrappers[0] = ActionResponseWrapper.Union(
        //         PromptResponse.Action(requester, true),
        //         new ActionResponseWrapper[] { }
        //     );
        //     
        //     DoAction(true);
        //     DeclaredRoundEnds.Add(requester);
        //     if (DeclaredRoundEnds.Count != 2)
        //         return wrappers;
        //     
        //     ActingId = ulong.MaxValue;
        //     FirstId = DeclaredRoundEnds.First();
        //     
        //     wrappers[1] = ActionResponseWrapper.Union(
        //         PromptResponse.Phase("end"),
        //         new ActionResponseWrapper[] { }
        //     );
        //     
        //     Game.ResolveManager.PassiveTrigger(Timing.OnEndPhase, FirstActive);
        //
        //     for (var i = 0; i < 2; i++)
        //         Game.Logics[i].Resource.Clear();
        //
        //     var drawResponses = DeclaredRoundEnds
        //         .Select(playerId =>
        //         {
        //             var i = Game.MappingId[playerId];
        //             var logic = Game.Logics[i];
        //             
        //             var (drewList, overdrewList) = logic.DeckCard.Draw(2);
        //             logic.DeckCard.Append(drewList);
        //             return new DrawResponse(playerId, drewList, overdrewList);
        //         })
        //         .Cast<IActionResponse>()
        //         .ToArray();
        //
        //     
        //     DeclaredRoundEnds.Clear();
        //
        //     wrappers[2] = ActionResponseWrapper.Union(
        //         drawResponses,
        //         new ActionResponseWrapper[] { }
        //     );
        //     
        //     return wrappers;
        // }
        
        public ActionResponseWrapper[] DeclareEndRound(ulong requester, out bool roundEnd)
        {
            roundEnd = false;
            
            var onDeclareRoundEnd = ActionResponseWrapper.Union(
                PromptResponse.Action(requester, true),
                Resolve.PassiveTrigger(Timing.OnDeclareRoundEnd, Acting)
            );
            
            DoAction(true);
            DeclaredRoundEnds.Add(requester);
            
            if (DeclaredRoundEnds.Count != 2)
                return onDeclareRoundEnd;

            roundEnd = true;
            ActingId = ulong.MaxValue;
            FirstId = DeclaredRoundEnds.First();
            
            var onEndPhase = ActionResponseWrapper.Union(
                PromptResponse.Phase("end"),
                Resolve.PassiveTrigger(Timing.OnEndPhase)
            );

            for (var i = 0; i < 2; i++)
                Game.Logics[i].Resource.Clear();

            var endDraw = DeclaredRoundEnds
                .SelectMany(playerId =>
                {
                    var player = Game.RequesterLogic(playerId);
                    var effect = new DrawEffect(Site.Self, DrawMode.Top, 2);
                    return Resolve.PassiveTrigger(effect, player);
                })
                .ToArray();
            
            DeclaredRoundEnds.Clear();

            var onRoundEnd = Resolve.PassiveTrigger(Timing.OnRoundEnd);
            
            return onDeclareRoundEnd.Concat(onEndPhase).Concat(endDraw).Concat(onRoundEnd).ToArray();
        }
    }
}