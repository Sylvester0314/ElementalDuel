using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.GameLogic;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public enum SwitchSignal
    {
        Result, // Return the switch result
        Draw,   // Draw a card from the deck and switch
        Back,   // First card move to hand callback
        Hand    // Select a card from the hand and switch
    }

    public class SwitchCardResponse : BaseResponse, IEquatable<SwitchCardResponse>
    {
        public int Amount;
        public bool IsStarting;
        public List<ActionCardInformation> SwitchedList;
        public SwitchSignal SwitchSignal;

        public SwitchCardResponse()
        {
            SwitchedList = ActionCardInformation.EmptyList;
        }

        public SwitchCardResponse(ulong id, SwitchSignal signal, List<ActionCard> cards, bool starting) : base(id)
        {
            SwitchedList = cards.Select(card => card.Convert()).ToList();
            SwitchSignal = signal;
            Amount = SwitchedList.Count;
            IsStarting = starting;
        }
        
        public static SwitchCardResponse Starting(ulong id)
            => new (id, SwitchSignal.Draw, ActionCard.EmptyList, true);
        
        public static SwitchCardResponse Back(ulong id)
            => new (id, SwitchSignal.Back, ActionCard.EmptyList, true);

        public override async void Process()
        {
            if (IsRequester)
                await Self();
            else
                await Opponent();
            
            base.Process();
        }

        private async Task Self()
        {
            Func<Task> action = SwitchSignal switch
            {
                SwitchSignal.Hand   => async () => await Global.hand.MoveToBuffer(),
                SwitchSignal.Draw   => async () => await Global.buffer.StartSwitchCard(IsStarting),
                SwitchSignal.Back   => async () => await Global.buffer.SwitchingCardsMoveToHand(IsStarting),
                SwitchSignal.Result => async () => await Global.buffer.SwitchCards(SwitchedList, IsStarting),
                _                   => throw new ArgumentOutOfRangeException()
            };

            await action.Invoke();
        }

        private async Task Opponent()
        {
            if (SwitchSignal != SwitchSignal.Result)
                return;

            await Global.oppoHand.SwitchCards(Amount);
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);

            serializer.SerializeValue(ref SwitchSignal);
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref IsStarting);
            NetCodeMisc.SerializeList(serializer, ref SwitchedList);
        }

        public bool Equals(SwitchCardResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return SwitchSignal == other.SwitchSignal &&
                   Amount == other.Amount &&
                   IsStarting == other.IsStarting &&
                   SwitchedList.Equals(other.SwitchedList);
        }
    }
}