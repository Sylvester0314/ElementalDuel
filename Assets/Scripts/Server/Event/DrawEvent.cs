using System.Collections.Generic;
using System.Text;
using Client.Logic.Response;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Misc;

namespace Server.Logic.Event
{
    public class DrawEvent : BaseEvent
    {
        public List<ActionCard> Drew;
        public List<ActionCard> Overdrew;

        public DrawEvent(
            IEventSource source, IEventTarget target, IEventGenerator via,
            List<ActionCard> drew, List<ActionCard> overdrew
        ) : base(source, target, via)
        {
            Drew = drew;
            Overdrew = overdrew;
        }

        public override List<BaseEvent> Execute(ResolveTree resolve)
        {
            Receiver.DeckCard.Append(Drew);
            resolve.Events.Add(this);
            
            return EmptyList;
        }

        public override IReadOnlyList<IActionResponse> ToResponses()
            => new DrawResponse(Receiver.Id, Drew, Overdrew).SingleList();

        public override void Log() => 
            AppendInformation(
                AppendInformation(Logger, Drew, "Drew"), 
                Overdrew, "Overdrew"
            ).Print();

        private StringBuilder AppendInformation(StringBuilder logger, List<ActionCard> cards, string label)
        {
            logger.Append($"{label} ").Append(cards.Count).AppendLine(" Card(s):");

            if (cards.Count == 0)
                return logger;
            
            foreach (IEventSource card in cards)
                logger.Append("- ").AppendLine(card.LocalizedName);
            
            return logger;
        }
    }
}