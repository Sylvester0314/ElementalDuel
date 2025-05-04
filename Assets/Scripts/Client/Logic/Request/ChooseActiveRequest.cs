using System;
using System.Text;
using System.Threading.Tasks;
using Client.Logic.Response;
using DG.Tweening;
using Shared.Handler;
using Shared.Misc;
using Unity.Netcode;
using UnityEngine;

namespace Client.Logic.Request
{
    public class ChooseActiveRequest : BaseRequest, IEquatable<ChooseActiveRequest>
    {
        public bool IsStarting;
        public string CharacterId;
        
        public ChooseActiveRequest() { }

        public ChooseActiveRequest(string id, bool starting = false)
        {
            CharacterId = id;
            IsStarting = starting;
        }

        public override async Task Process()
        {
            if (IsStarting)
                Starting();
            else
                SingleDefeated();
            
            await base.Process();
        }

        private void Starting()
        {
            var wrappers = Logic.CharacterLogic.SwitchActive(CharacterId);
            
            Game.ControlSynchronousOperation(
                "choose_active",
                () => BothComplete(wrappers),
                () => HalfComplete(wrappers)
            );
        }

        private void SingleDefeated()
        {
            var node = Game.BranchesTemp[CharacterId];

            var promptResponse = PromptResponse.Close(true);
            var closeWrapper = ActionResponseWrapper.CreateUnblock(promptResponse);
            Response(closeWrapper);
            
            var wrappers = ActionResponseWrapper.Resume(node.Wrappers);
            Response(wrappers);
            
            Game.ResolveManager.MergeResultToManager(node);
            Game.RefreshCostAndResolutionCaches();
        }
        
        private void BothComplete(ActionResponseWrapper[][] wrappers)
        {
            var opponent = Game.RequesterLogic(RequesterId).Opponent;
            var oppoActive = opponent.ActiveCharacter.UniqueId;
            
            var oppoActiveResponse = new SwitchActiveResponse(opponent.Id, oppoActive);
            var hideBannerResponse = PromptResponse.Close(true);
            
            Response(wrappers[0]);
            // Notification to both players regarding the active character of the Second Player
            Response(wrappers[1]);
            // Notification to your site regarding the opponent's site's active character
            TargetResponse(oppoActiveResponse);
            TargetResponse(hideBannerResponse, opponent.Id);

            DOVirtual.DelayedCall(0.5f, Game.StartNewRound);
        }

        private void HalfComplete(ActionResponseWrapper[][] wrappers)
        {
            const string entry = "waiting_opponent_select_first_active";
            var waitingResponse = PromptResponse.FixedBanner(entry);
                
            // TODO 使用 ResolveManager 的被动触发器
            // wrappers[0] 表示在切换角色前（时）涉及的切换角色引发的结算，即绒翼龙
            // 不过由于目前游戏中不存在被动切人时引发的此类结算，
            // 尤其是在双死情况下的结算情况，结算效果暂时不明
            TargetResponse(wrappers[0]);
            TargetResponse(wrappers[1]);
            TargetResponse(waitingResponse);
                
            Game.SaveResponseTemporary(RequesterId, wrappers[2]);
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref IsStarting);
            serializer.SerializeValue(ref CharacterId);
        }

        public bool Equals(ChooseActiveRequest other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return IsStarting == other.IsStarting && CharacterId.Equals(other.CharacterId);
        }

        public override string ToString()
        {
            return $"ChooseActive(IsStarting={IsStarting}, Character={CharacterId})";
        }
    }
}