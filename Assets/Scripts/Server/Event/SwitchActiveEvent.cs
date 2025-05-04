using System.Collections.Generic;
using Client.Logic.Response;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Misc;

namespace Server.Logic.Event
{
    public class SwitchActiveEvent : BaseEvent
    {
        public SwitchActiveEvent(IEventSource source, IEventTarget target, IEventGenerator via) 
            : base(source, target, via) { }

        public override List<BaseEvent> Execute(ResolveTree resolve)
        {
            if (Target is not CharacterData character)
                return EmptyList;
            
            var logic = character.Belongs.CharacterLogic;
            if (logic.Active != null)
                logic.Active.IsActive = false;
            
            character.IsActive = true;
            logic.Active = character;
            resolve.Events.Add(this); 
            
            return resolve.Context.CatchEvents(this, Timing.AfterCharacterSwitched);
        }

        public override IReadOnlyList<IActionResponse> ToResponses()
            => new SwitchActiveResponse(Source.Belongs.Id, Target.UniqueId).SingleList();

        public override void Log() => Logger.Print();
    }
}