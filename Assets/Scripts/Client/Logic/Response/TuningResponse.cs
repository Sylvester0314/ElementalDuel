using System;
using System.Collections.Generic;
using Server.GameLogic;
using Shared.Enums;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public enum TuningAction
    {
        Start,
        Finish
    }

    public class TuningResponse : BaseResponse, IEquatable<TuningResponse>
    {
        public TuningAction Action;

        public int CardTimestamp;
        public DiceLogic Dice;
        public CostType Element;
        public List<string> DisableIds;

        public TuningResponse()
        {
            Dice = new DiceLogic();
            DisableIds = StaticMisc.EmptyStringList;
        }

        public TuningResponse(ulong id, TuningAction action, int timestamp) : base(id)
        {
            Action = action;
            CardTimestamp = timestamp;
        }

        public override void Process()
        {
            Action call = IsRequester ? Self : Opponent;

            call.Invoke();
            base.Process();
        }

        public void Self()
        {
            var df = Global.diceFunction;
            // TODO 动画 - 我方元素骰子调和动画
            if (Action == TuningAction.Start)
                df.PreviewElementalTuning(Dice, DisableIds, Element, CardTimestamp);
            if (Action == TuningAction.Finish)
                df.DoElementalTuning(Dice, CardTimestamp);
        }

        public void Opponent()
        {
            if (Action == TuningAction.Finish)
                Global.oppoHand.RemoveCard();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);

            serializer.SerializeValue(ref Action);
            serializer.SerializeValue(ref CardTimestamp);
            serializer.SerializeValue(ref Dice);
            serializer.SerializeValue(ref Element);
            NetCodeMisc.SerializeList(serializer, ref DisableIds);
        }

        public bool Equals(TuningResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return CardTimestamp == other.CardTimestamp && Dice.Id == other.Dice.Id;
        }
    }
}