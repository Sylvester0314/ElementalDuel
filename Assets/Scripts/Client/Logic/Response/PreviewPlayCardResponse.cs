// using System;
// using System.Collections.Generic;
// using Client.Logic.Request;
// using Shared.Misc;
// using Unity.Netcode;
//
// namespace Client.Logic.Response
// {
//     public class PreviewPlayCardResponse : BaseResponse, IEquatable<PreviewPlayCardResponse>
//     {
//         public int Timestamp;
//         public List<string> BannerHint;
//         
//         public PreviewPlayCardResponse() { }
//
//         public PreviewPlayCardResponse(ulong id, int timestamp, List<string> hint) : base(id)
//         {
//             Timestamp = timestamp;
//             BannerHint = hint;
//         }
//
//         public override void Process()
//         {
//             var card = Global.hand.GetCard(Timestamp);
//
//             ValueTuple<string, Action> param = ("play_card", () =>
//             {
//                 var dices = Global.diceFunction.GetSelectingDices();
//                 Request(PlayCardRequest.Use(Timestamp, dices));
//             });
//
//             Global.hand.usingCard = true;
//             Global.prompt.banner.Display(ParseHint());
//             Global.prompt.button.Display(param);
//             Global.diceFunction.OpenChooseDiceUI(card.matched.dices);
//             
//             base.Process();
//         }
//
//         private ValueTuple<string, string> ParseHint()
//         {
//             var s = BannerHint;
//             if (BannerHint.Count == 1)
//                 return (ResourceLoader.GetLocalizedUIText(s[0]), string.Empty);
//             
//             var p1 = ResourceLoader.GetLocalizedUIText(s[0]);
//             var p2 = ResourceLoader.GetLocalizedValue(s[1], s[2]);
//             return (p1, p2);
//         }
//
//         public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
//         {
//             base.NetworkSerialize(serializer);
//
//             serializer.SerializeValue(ref Timestamp);
//             NetCodeMisc.SerializeList(serializer, ref BannerHint);
//         }
//
//         public bool Equals(PreviewPlayCardResponse other)
//         {
//             if (ReferenceEquals(other, null) || !base.Equals(other))
//                 return false;
//
//             return BannerHint.Equals(other.BannerHint);
//         }
//     }
// }