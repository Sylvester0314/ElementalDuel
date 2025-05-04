using System.Collections.Generic;
using Client.Logic.Response;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Misc;

namespace Server.Logic.Event
{
    public class GenerateDiceEvent : BaseEvent
    {
        public List<DiceLogic> Generated;

        public GenerateDiceEvent(
            IEventSource source, IEventTarget target, IEventGenerator via,
            List<DiceLogic> generated
        ) : base(source, target, via)
        {
            Generated = generated;
        }

        public override List<BaseEvent> Execute(ResolveTree resolve)
        {
            Receiver.Resource.Append(Generated);
            resolve.Events.Add(this);
            
            return EmptyList;
        }

        public override IReadOnlyList<IActionResponse> ToResponses()
            => ResourceResponse.AppendDices(Receiver.Id, Generated).SingleList();

        public override void Log()
        {
            var logger = Logger
                .Append("Generated ")
                .Append(Generated.Count)
                .AppendLine(" Dice(s):");

            foreach (var dice in Generated)
                logger.Append("- ").AppendLine(dice.Type.ToString());
            
            logger.Print();
        }
    }
}