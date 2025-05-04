using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Logic.Response;
using Server.GameLogic;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Request
{
    public class PlayCardRequest : BaseRequest, IEquatable<PlayCardRequest>
    {
        public int Timestamp;
        public bool IsPreview;
        public string Target;
        public List<string> Dices;

        public PlayCardRequest()
        {
            Dices = StaticMisc.EmptyStringList;
        }
        
        public PlayCardRequest(int timestamp, bool preview, List<string> dices, string target)
        {
            Timestamp = timestamp;
            IsPreview = preview;
            Dices = dices;
            Target = target;
        }

        #region Factory Methods

        public static PlayCardRequest Preview(int timestamp)
            => new (timestamp, true, StaticMisc.EmptyStringList, string.Empty);

        public static PlayCardRequest Use(int timestamp, List<string> dices, string target)
            => new (timestamp, false, dices, target);

        #endregion

        public override async Task Process()
        {
            if(!Logic.DeckCard.Hand.TryGetValue(Timestamp, out var card))
            {
                var promptResponse = PromptResponse.Dialog("no_valid_target");
                TargetResponse(promptResponse);
                return;
            }
            
            if (IsPreview)
                Preview(card);
            else
                Use(card);
            
            await base.Process();
        }

        private void Preview(ActionCard card)
        {
            var subTree = Game.ResolveManager.CreateSubTree(Logic, card);
            var overviews = subTree.Overviews;
            var hint = card.BannerHint;
            
            var response = PlayCardResponse.Preview(RequesterId, Timestamp, hint, overviews);
            
            TargetResponse(response);
            Game.Receiver.Dequeue(UniqueId);
        }

        private void Use(ActionCard card)
        {
            var tm = Game.TurnManager;
            if (!tm.Check(RequesterId, UniqueId))
                return;
            
            var valid = Logic.Resource.Check(card.Cost, Dices);
            var response = PlayCardResponse.Use(RequesterId, card, valid);
            Response(response);
            
            if (!valid)
            {
                Game.Receiver.Dequeue(UniqueId);
                return;
            }

            tm.DoAction(card.CombatAction);
            
            var root = Game.ResolveManager.GetSubTreeRoot(card.Key, Target);
            
            var useCostResponse = ResourceResponse.Use(root, card, Dices);
            var wrappers = ActionResponseWrapper.Union(useCostResponse, root.Wrappers);
            Response(wrappers);

            Game.ResolveManager.MergeResultToManager(root);
            Logic.DeckCard.Remove(Timestamp);

            tm.AfterAction(false);
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref IsPreview);
            serializer.SerializeValue(ref Timestamp);
            serializer.SerializeValue(ref Target);
            NetCodeMisc.SerializeList(serializer, ref Dices);
        }

        public bool Equals(PlayCardRequest other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return IsPreview == other.IsPreview &&
                   Timestamp == other.Timestamp &&
                   Target.Equals(other.Target);
        }
        
        public override string ToString()
        {
            return $"PlayCard(Preview={IsPreview}, Timestamp={Timestamp}, Target={Target})";
        }
    }
}