using System;
using Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using Server.GameLogic;
using Server.Logic.Event;
using Shared.Handler;
using Shared.Logic.CharacterFilter;
using Shared.Misc;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shared.Logic.Effect
{
    [Serializable]
    public abstract class BaseCreateEffect : BaseEffect
    {
        [BoxGroup("Target Selection")]
        public Site site = Site.Opponent;
        
        [BoxGroup("Target Selection"), HideIf("ModeHideCondition")]
        public TargetMode mode = TargetMode.First;
        
        [BoxGroup("Target Selection"), HideIf("TargetHideCondition")]
        public TargetType target = TargetType.Active;

        [BoxGroup("Target Selection"), ShowIf("StandByCondition")]
        public bool mustStandby;
        
        [BoxGroup("Target Selection"), HideIf("TargetHideCondition"), SerializeReference]
        public List<BaseFilter> filters = new ();
        
        // The character parameter is used to obtain "the character itself" or
        // "the closest opponent's character to the character"
        public List<CharacterData> GetTargets(PlayerLogic logic, CharacterData character = null)
        {
            if (character != null && target == TargetType.Self)
                return character.SingleList();
            
            var characters = new List<CharacterData>();
            var self = logic.CharacterLogic;
            var oppo = logic.Opponent.CharacterLogic;
            
            if (site != Site.Opponent)      // Self or Both
                characters.AddRange(self.GetTargetedCharacters(target, mustStandby));
            if (site != Site.Self)          // Opponent or Both
                characters.AddRange(oppo.GetTargetedCharacters(target, mustStandby));

            if (filters?.Count > 0)
                characters = filters.Aggregate(
                    characters.AsEnumerable(),
                    (result, filter) => result.Where(filter.Check)
                ).ToList();
            
            return mode switch
            {
                TargetMode.First         => characters.Take(1).ToList(),
                TargetMode.AllCharacters => characters,
                _                        => characters
            };
        }

        protected abstract IReadOnlyList<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via);

        public List<BaseEvent> GenerateEvents(IEventSource source, IEventGenerator via, Guid id)
            => GenerateEvents(source, via)
                .Select(e => e.UnifiedId(id))
                .ToList();

        private bool Self => target is TargetType.Self;
        private bool NonSite => site is Site.None;
        private bool NonTarget => this is BaseNonTargetCreateEffect;
        private bool GenerateStatusExtend => this is GenerateStatusEffect generate 
                                             && generate.statusAsset.type is StatusType.CombatStatus;
        private bool ModeHideCondition => NonTarget || NonSite || GenerateStatusExtend || Self ||
                                          target is TargetType.ExtraZone;
        private bool TargetHideCondition => NonTarget || NonSite || GenerateStatusExtend || Self;
        private bool StandByCondition => !TargetHideCondition && target is not 
            (TargetType.Adjacent or TargetType.Dead or TargetType.Active or
            TargetType.ExtraZone or TargetType.Self);
    }

    public abstract class BaseNonTargetCreateEffect : BaseCreateEffect { }
    
    public abstract class AttributeModifiableEffect : BaseCreateEffect { }
}