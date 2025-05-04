using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Logic.Request;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class PlayCardResponse : BaseResponse, IEquatable<PlayCardResponse>
    {
        public bool IsPreview;
        public int Timestamp;
        
        public string CardAsset;
        public List<string> BannerHint;
        public Dictionary<string, ResolveOverview> Overviews;

        public bool Valid;
        
        public PlayCardResponse()
        {
            Overviews = new Dictionary<string, ResolveOverview>();
            BannerHint = StaticMisc.EmptyStringList;
        }

        public PlayCardResponse(ulong id, bool preview, int timestamp) : base(id)
        {
            IsPreview = preview;
            Timestamp = timestamp;
            CardAsset = string.Empty;
            BannerHint = StaticMisc.EmptyStringList;
            Overviews = new Dictionary<string, ResolveOverview>();
        }

        #region Factory Methods

        public static PlayCardResponse Preview(
            ulong id, int timestamp, List<string> hint, 
            Dictionary<string, ResolveOverview> overviews) 
            => new (id, true, timestamp) { BannerHint = hint, Overviews = overviews };

        public static PlayCardResponse Use(ulong id, ActionCard card, bool valid)
            => new (id, false, card.Timestamp) { Valid = valid, CardAsset = card.EntityName };

        #endregion

        public override async void Process()
        {
            if (IsPreview)
                OpenPreviewUI();
            else if (Valid)
                await ValidHandler();
            else if (IsRequester)
            {
                var card = Global.hand.GetCard(Timestamp);
                Global.prompt.dialog.Display(card.SynchronousCost);
            }
         
            base.Process();
        }

        private void OpenPreviewUI()
        {
            if (Overviews.Count == 0)
            {
                Global.prompt.dialog.DisableShowDark();
                Global.prompt.dialog.Display("no_valid_target");
                Global.hand.gameObject.SetActive(true);
                Global.hand.ExtendAreaLayout();
                return;
            }
            
            var card = Global.hand.GetCard(Timestamp);

            ValueTuple<string, Action> param = ("play_card", RequestPlay);

            Global.Overviews = Overviews;
            Global.hand.usingCard = true;
            Global.prompt.banner.Display(ParseHint());
            Global.prompt.button.Display(param);
            Global.diceFunction.OpenChooseDiceUI(card.matched.dices);

            var first = Overviews.Keys.First();

            if (first != ResolveTree.Root)
            {
                Global.previewingMainTarget = Global.GetCharacter(first);
                Global.previewingMainTarget.PreviewingAction = RequestPlay;
            }
            Global.OpenPreviewUI(first);
        }
        
        private void RequestPlay()
        {
            var dices = Global.diceFunction.GetSelectingDices();
            var target = Global.previewingMainTarget?.uniqueId ?? ResolveTree.Root;
            var request = PlayCardRequest.Use(Timestamp, dices, target);
                
            Request(request);
        }

        private async Task ValidHandler()
        {
            if (!IsRequester)
                Global.oppoHand.RemoveCard();
            else
            {
                Global.hand.RemoveActionCard(Timestamp);
                Global.diceFunction.ResetLayout();
                Global.prompt.CloseAll();
                Global.CancelPreview();
                Global.SetTurnInformationStatus(true);
            }

            var completion = new TaskCompletionSource<bool>();
            var asset = await ResourceLoader.LoadSoAsset<ActionCardAsset>(CardAsset);
            
            Player.usingDisplay.Display(
                asset.cardImage,
                () => completion.TrySetResult(true)
            );
            
            await completion.Task;
        }

        private ValueTuple<string, string> ParseHint()
        {
            var s = BannerHint;
            if (BannerHint.Count == 1)
                return (ResourceLoader.GetLocalizedUIText(s[0]), string.Empty);

            var p1 = ResourceLoader.GetLocalizedUIText(s[0]);
            var p2 = ResourceLoader.GetLocalizedValue(s[1], s[2]);
            return (p1, p2);
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref IsPreview);
            serializer.SerializeValue(ref Timestamp);
            serializer.SerializeValue(ref CardAsset);
            serializer.SerializeValue(ref Valid);
            
            NetCodeMisc.SerializeList(serializer, ref BannerHint);
            NetCodeMisc.SerializeDictionary(serializer, ref Overviews);
        }

        public bool Equals(PlayCardResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return IsPreview == other.IsPreview &&
                   Timestamp == other.Timestamp &&
                   Valid == other.Valid &&
                   CardAsset.Equals(other.CardAsset);
        }
    }
}