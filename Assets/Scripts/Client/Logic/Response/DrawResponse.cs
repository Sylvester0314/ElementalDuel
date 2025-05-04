using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class DrawResponse : BaseResponse, IEquatable<DrawResponse>
    {
        public int Amount;
        public List<ActionCardInformation> DrewList;
        public List<ActionCardInformation> Overdrew;
        public bool DrawToHand;
        
        public DrawResponse()
        {
            DrewList = ActionCardInformation.EmptyList;
            Overdrew = ActionCardInformation.EmptyList;
        }

        public DrawResponse(ulong id, List<ActionCard> drewList, List<ActionCard> overdrew) : base(id)
        {
            DrawToHand = true;
            DrewList = drewList.Select(card => card.Convert()).ToList();
            Overdrew = overdrew.Select(card => card.Convert()).ToList();
            Amount = DrewList.Count;
        }

        public static DrawResponse Starting(ulong id, List<ActionCard> cards)
            => new ()
            {
                RequesterId = id,
                Overdrew = ActionCardInformation.EmptyList,
                DrewList = cards.Select(card => card.Convert()).ToList(),
                DrawToHand = false,
                Amount = cards.Count
            };

        public override async void Process()
        {
            var deck = Player.deck;
            
            if (!IsRequester)
                await deck.DrawToOpponentHand(Amount);
            else if (DrawToHand)
                await deck.DrawToSelfHand(DrewList, Overdrew);
            else
                await deck.Draw(DrewList, false);
            
            base.Process();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);

            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref DrawToHand);
            NetCodeMisc.SerializeList(serializer, ref DrewList);
            NetCodeMisc.SerializeList(serializer, ref Overdrew);
        }

        public bool Equals(DrawResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Amount == other.Amount && 
                   DrewList.Equals(other.DrewList) && 
                   Overdrew.Equals(other.Overdrew);
        }
    }
}