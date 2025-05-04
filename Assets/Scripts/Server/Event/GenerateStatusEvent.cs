using System.Collections.Generic;
using Client.Logic.Response;
using Server.ResolveLogic;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Effect;
using Shared.Logic.Statuses;
using Shared.Misc;

namespace Server.Logic.Event
{
    public class GenerateStatusEvent : PreviewableEvent
    {
        public GenerateStatusEffect Effect;
        public StatusLogic Zone;
        
        public Status Generated;

        private string Id => Zone.UniqueId;

        public GenerateStatusEvent(
            IEventSource source, IEventTarget target, IEventGenerator via,
            StatusLogic zone, GenerateStatusEffect effect
        ) : base(source, target, via)
        {
            Zone = zone;
            Effect = effect;
        }

        public override List<BaseEvent> Execute(ResolveTree resolve)
        {
            Generated = Zone.Append(Effect);
            resolve.Events.Add(this);

            return EmptyList;
        }

        public override IReadOnlyList<IActionResponse> ToResponses()
            => StatusResponse.Generate(Id, Generated).SingleList();

        public override void WriteToOverview(ResolveOverview overview)
        {
            if (Generated.Type is StatusType.Status or StatusType.CombatStatus)
                return;
            
            overview.StatusModifications.TryAdd(Id, new StatusModification());
            overview.StatusModifications[Id].Statuses.Add(Generated);
        }
        
        public override void Log() => Logger
            .Append(ResourceLoader.GetLocalizedCard(Effect.statusAsset.statusName))
            .Print();
    }
}