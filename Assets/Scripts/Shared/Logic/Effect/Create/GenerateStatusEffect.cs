using Server.Logic.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Statuses;
using Sirenix.OdinInspector;

namespace Shared.Logic.Effect
{
    [Serializable]
    public class GenerateStatusEffect : BaseCreateEffect
    {
        [BoxGroup("Effect Configurations"), Required]
        public StatusCardAsset statusAsset;

        [BoxGroup("Effect Configurations/Override Usages"), HideLabel]
        [ShowIf("@Overrideable(StatusLifeMode.Usages)")]
        public StatusLifeSetting usages = new (true);
        
        [BoxGroup("Effect Configurations/Override Durations"), HideLabel]
        [ShowIf("@Overrideable(StatusLifeMode.Durations)")]
        public StatusLifeSetting durations = new (true);
        
        protected override IReadOnlyList<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via)
        {
            var logic = source.Belongs;
            var character = (via as SkillLogic)?.Owner;
            var zones = statusAsset.type switch
            {
                StatusType.CombatStatus => GetZones(logic, p => p.CombatStatuses),
                StatusType.Summon       => GetZones(logic, p => p.SummonZone),
                StatusType.Support      => GetZones(logic, p => p.SupportZone),
                _                       => GetTargets(logic, character).Select(data => data.Statuses).ToList()
            };
                
            return zones.Select(
                zone => new GenerateStatusEvent(source, zone.Belongs, via, zone, this)
            ).ToList();
        }

        private List<StatusLogic> GetZones(PlayerLogic logic, Func<PlayerLogic, StatusLogic> getter)
        {
            var zones = new List<StatusLogic>();
            
            if (site != Site.Opponent)      // Self or Both
                zones.Add(getter(logic));
            if (site != Site.Self)          // Opponent or Both
                zones.Add(getter(logic.Opponent));
            
            return zones;
        }

        public override IReadOnlyList<BaseEvent> ResponseEvent(Status handler, BaseEvent e)
            => GenerateEvents(handler, e.Via).Concat(AutoConsume(handler)).ToList();

        private bool Overrideable(StatusLifeMode lifeMode)
            => statusAsset != null && statusAsset.canOverride && 
               statusAsset.mode.HasFlag(lifeMode);
    }
}