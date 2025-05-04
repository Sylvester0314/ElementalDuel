using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Enums;
using Shared.Misc;
using Sirenix.Utilities;
using DrawResult = System.ValueTuple<
    System.Collections.Generic.List<Server.GameLogic.ActionCard>,
    System.Collections.Generic.List<Server.GameLogic.ActionCard>
>;

namespace Server.GameLogic
{
    public enum InsertEvenlyMode
    {
        Whole,
        Top,
        Bottom
    }

    public class DeckCardLogic
    {
        public PlayerLogic PlayerLogic;

        public int Timestamp;
        public List<ActionCard> Cards;
        public Dictionary<int, ActionCard> Hand;

        public DeckCardLogic() { }
        
        public DeckCardLogic(PlayerLogic logic, List<ActionCardAsset> assets)
        {
            PlayerLogic = logic;
            Hand = new Dictionary<int, ActionCard>();

            Cards = assets.Select(asset => new ActionCard(PlayerLogic, asset)).ToList();
            Cards.Shuffle(PlayerLogic.Game.Random);
        }

        #region Operation

        public ActionCard Remove(int timestamp)
        {
            var card = Hand.RemoveAndGet(timestamp);
            Cards.Remove(card);

            return card;
        }
        
        public DrawResult Draw(int amount, List<ActionCard> deck = null, bool bottom = false)
        {
            deck ??= Cards;
            amount = Math.Min(amount, deck.Count);

            if (amount <= 0)
                return (ActionCard.EmptyList, ActionCard.EmptyList);
            
            var drawAmount = Math.Min(amount, 10 - Hand.Count);
            var drew = GetActionCards(deck, drawAmount, bottom);
            
            var overdrawAmount = amount - drawAmount;
            var overdrew = overdrawAmount > 0
                ? GetActionCards(deck, overdrawAmount, bottom)
                : ActionCard.EmptyList;
            
            drew.Concat(overdrew).ForEach(card => Cards.Remove(card));

            return (drew, overdrew);
        }

        public void Append(List<ActionCard> cards)
        {
            cards.ForEach(card => Hand.Add(card.Timestamp, card));
        }

        public DrawResult DrawBySpecificName(int amount, string name, List<ActionCard> deck = null)
        {
            deck ??= Cards;
            return Draw(amount, deck.FindAll(card => card.MainCardName == name));
        }
        
        public DrawResult DrawBySpecificType(int amount, Property property, List<ActionCard> deck = null)
        {
            deck ??= Cards;
            return Draw(amount, deck.FindAll(card => card.Asset.Properties.Contains(property)));
        }

        public List<ActionCard> Switch(List<int> timestamps)
        {
            var cards = new List<ActionCard>();
            foreach (var timestamp in timestamps)
            {
                cards.Add(Hand[timestamp]);
                Hand.Remove(timestamp);
            }
            
            cards.ForEach(card => InsertRandomly(card));

            // Create the whitelist
            var blackList = new HashSet<string>(cards.Select(card => card.EntityName));
            var whiteList = Cards
                .Where(card => !blackList.Contains(card.EntityName))
                .ToList();

            var amount = cards.Count;
            var (result, _) = Draw(amount, whiteList);

            // If the number of cards in the whitelist is less than the number of
            // cards to be switched, draw cards from the remaining deck
            var diffAmount = amount - result.Count;
            if (diffAmount > 0)
                result.AddRange(Draw(diffAmount).Item1);

            Append(result);
            
            return result;
        }

        public void InsertRandomly(
            ActionCard card, int insertRange = 0,
            InsertEvenlyMode mode = InsertEvenlyMode.Whole
        )
        {
            var (start, length) = CalculateRange(insertRange, mode);
            var index = PlayerLogic.Game.Random.NextInt(start, length);

            Cards.Insert(index, card);
        }

        public void InsertEvenly(
            List<ActionCard> cards, int insertRange = 0,
            InsertEvenlyMode mode = InsertEvenlyMode.Whole
        )
        {
            var (start, length) = CalculateRange(insertRange, mode);
            var indexes = SplitEvenly(length, cards.Count);

            var index = indexes[0] + start;
            for (var i = 1; i < indexes.Count; i++)
            {
                Cards.Insert(index, cards[i - 1]);
                index += indexes[i] + 1;
            }
        }
        
        #endregion

        #region Misc
        
        private List<ActionCard> GetActionCards(List<ActionCard> deck, int amount, bool bottom)
        {
            var result = bottom
                ? deck.GetRange(deck.Count - amount, amount).ReversedList()
                : deck.GetRange(0, amount);
            result.ForEach(card => card.Timestamp = Timestamp++);
            return result;
        }

        private ValueTuple<int, int> CalculateRange(int range, InsertEvenlyMode mode)
        {
            var length = mode == InsertEvenlyMode.Whole ? Cards.Count : range;
            var start = mode == InsertEvenlyMode.Bottom ? Cards.Count - range : 0;
            return (start, length + 1);
        }

        private List<int> SplitEvenly(int length, int parts)
        {
            var chunkNumber = parts + 1;
            var remainder = length % chunkNumber;
            var divisor = (int)Math.Floor(1f * length / chunkNumber);

            var result = new List<int>();
            for (var i = 0; i < chunkNumber; i++)
            {
                var value = remainder-- > 0 ? 1 : 0;
                result.Add(divisor + value);
            }

            return result;
        }

        public DeckCardLogic Clone(PlayerLogic logic)
        {
            var clone = new DeckCardLogic
            {
                PlayerLogic = logic,
                Timestamp = Timestamp,
                Cards = new List<ActionCard>(),
                Hand = new Dictionary<int, ActionCard>()
            };

            foreach (var card in Cards)
                clone.Cards.Add(card.Clone(logic));
            foreach (var (timestamp, card) in Hand)
                clone.Hand.Add(timestamp, card.Clone(logic));

            return clone;
        }
        
        #endregion
    }
}