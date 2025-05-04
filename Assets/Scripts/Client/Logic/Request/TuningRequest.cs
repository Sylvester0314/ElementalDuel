using System;
using System.Linq;
using System.Threading.Tasks;
using Client.Logic.Response;
using Server.GameLogic;
using Shared.Enums;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Request
{
    public class TuningRequest : BaseRequest, IEquatable<TuningRequest>
    {
        public TuningAction Action;
        public int CardTimestamp;
        public DiceLogic Dice;

        public TuningRequest()
        {
            Dice = new DiceLogic();
        }

        public TuningRequest(TuningAction action, DiceLogic dice, int timestamp)
        {
            Action = action;
            Dice = dice;
            CardTimestamp = timestamp;
        }

        public override async Task Process()
        {
            var element = Logic.ActiveCharacter.Element;
            var dice = Logic.Resource.Dices[Dice.Id];
            
            var response = Action == TuningAction.Start 
                ? Start(element, dice)  
                : Result(element, dice);
            Response(response);
            
            Game.Receiver.Dequeue(UniqueId);
            await Task.CompletedTask;
            
            if (Action == TuningAction.Finish)
                Game.TurnManager.AfterAction(false);
        }

        public TuningResponse Start(CostType element, DiceLogic prior)
        {
            var disable = Logic.Resource.Dices
                .Values
                .Where(dice => dice.Type == element || dice.Type == CostType.Any)
                .Select(dice => dice.Id)
                .ToList();
            
            return new TuningResponse(RequesterId, Action, CardTimestamp)
            {
                Dice = prior,
                DisableIds = disable,
                Element = element,
            };
        }

        public TuningResponse Result(CostType element, DiceLogic choosing)
        {
            choosing.Tuning(element);
            
            return new TuningResponse(RequesterId, Action, CardTimestamp)
            {
                Dice = choosing,
                DisableIds = StaticMisc.EmptyStringList
            };
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref Action);
            serializer.SerializeValue(ref CardTimestamp);
            serializer.SerializeValue(ref Dice);
        }

        public bool Equals(TuningRequest other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return Dice.Id == other.Dice.Id &&
                   CardTimestamp == other.CardTimestamp &&
                   Action == other.Action;
        }

        public override string ToString()
        {
            return $"TuningRequest(Action: {Action}, Source: {Dice.Type}, Card: {CardTimestamp})";
        }
    }
}