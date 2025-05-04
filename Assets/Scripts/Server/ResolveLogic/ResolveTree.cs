using System;
using System.Collections.Generic;
using System.Linq;
using Client.Logic.Response;
using Server.GameLogic;
using Server.Logic.Event;
using Server.Managers;
using Shared.Enums;
using Shared.Handler;
using Shared.Logic.Effect;
using Shared.Misc;
using UnityEngine;
using Events = System.Collections.Generic.List<Server.Logic.Event.BaseEvent>;

namespace Server.ResolveLogic
{
    public class ResolveTree
    {
        public ReactionLogic Reaction;
        public PlayerLogic OriginState;
        
        public ResolveNode Resolving;
        public Dictionary<string, ResolveNode> Branches;

        public bool GameOver => Resolving.FailedPlayers.Count != 0;
        public Events Events => Resolving.Events;
        public PlayerLogic Context => Resolving.State;
        public Dictionary<string, ResolveOverview> Overviews => Branches
            .ToDictionary(pair => pair.Key, pair => pair.Value.Overview);

        public const string Root = "origin";

        public ResolveTree(GameManager manager, PlayerLogic player)
        {
            OriginState = player;
            Reaction = manager.ReactionLogic;
            Branches = new Dictionary<string, ResolveNode>();
        }

        public ResolveNode Trigger(Timing timing)
        {
            Resolving = new ResolveNode(OriginState);
            
            Process(PassiveEvent.Create(Context.ActiveCharacter), timing);
            Resolving.Analysis();
            
            return Resolving;
        }
        
        public ResolveNode Trigger(BaseCreateEffect effect)
        {
            Resolving = new ResolveNode(OriginState);

            var source = Context.ActiveCharacter;
            var events = ExportEvents(Root, effect.SingleList(), source, null);
            
            ExecuteEvents(events);
            Resolving.Analysis();
            
            return Resolving;
        }
        
        public void PlayCard(ActionCard origin)
        {
            var effects = origin.Effects.GetAvailableEffects(
                origin, origin, null, out var branches);

            foreach (var branchId in branches)
            {
                Resolving = new ResolveNode(OriginState);
                Context.MirrorCard(origin, out var card);
                Resolving.Initialize(card);
                
                Branches.Add(branchId, Resolving);
                
                var events = ExportEvents(branchId, effects, card, card);
                
                ExecuteEvents(events);
                
                Resolving.Analysis();
            }
        }

        public void UseSkill(SkillLogic origin)
        {
            var effects = origin.Effects.GetAvailableEffects(
                origin.Owner, origin, origin.Variables, out var branches);

            foreach (var branchId in branches)
            {
                Resolving = new ResolveNode(OriginState);
                Context.MirrorSkill(origin, out var skill);
                Resolving.Initialize(skill);
                
                Branches.Add(branchId, Resolving);
                
                var source = skill.Owner ?? Context.ActiveCharacter;
                var events = ExportEvents(branchId, effects, source, skill);
                
                ExecuteEvents(events, () =>
                {
                    skill.AddCount();
                    return skill.CanGainEnergy
                        ? ModifyEnergyEvent.Create(source, skill, 1)
                        : null;
                });
                
                if (skill.Type is not (SkillType.PassiveSkill or SkillType.SwitchActive or SkillType.Technique))
                    Process(PassiveEvent.Create(source), Timing.AfterSkillUsed);
                
                Resolving.Analysis();
            }
        }

        private void Process(BaseEvent e, Timing timing)
        {
            var events = e.Trigger.CatchEvents(e, timing);
            ExecuteEvents(events);
        }

        private void ExecuteEvents(Events events, Func<BaseEvent> useInitiativeSkill = null)
        {
            if (events.Count == 0 || GameOver)
                return;
            
            var executedEvents = events
                .SelectMany(e => e.Execute(this)).ToList();

            var modifyHealthEvents = new Events();
            var immuneDefeatEvents = new Events();
            var otherTriggerEvents = new Events();

            foreach (var e in executedEvents)
            {
                var fail = false;
                var belongsSet = e switch
                {
                    DamageEvent { DefeatCharacter: true } damage => damage
                        .HandleCharacterDefeated(modifyHealthEvents, immuneDefeatEvents, ref fail),
                    DamageEvent or HealEvent => modifyHealthEvents,
                    _                        => otherTriggerEvents
                };
                
                if (fail)
                    Resolving.FailedPlayers.Add(e.Receiver.Id);
                
                belongsSet?.Add(e);
            }

            if (Resolving.CheckGameOver())
                return;

            var gainEnergyEvent = useInitiativeSkill?.Invoke();
            if (gainEnergyEvent != null)
                otherTriggerEvents.Insert(0, gainEnergyEvent);
            
            foreach (var _ in immuneDefeatEvents)
                Debug.Log("TODO 免于被击倒相关事件");
            
            ExecuteEvents(otherTriggerEvents);

            var (defeatEventList, commonEventList) = modifyHealthEvents
                .SplitBy(e => e is DamageEvent { DefeatCharacter: true });

            commonEventList.ForEach(e => Process(e,
                e is HealEvent ? Timing.AfterHealReceived : Timing.AfterDamageTaken
            ));
            defeatEventList.ForEach(e => Process(e, Timing.AfterDamageTaken));
            
            if (defeatEventList.Count != 0)
                HandleChooseActive();
        }
        
        private Events ExportEvents(
            string branchId, List<BaseCreateEffect> effects, 
            IEventSource source, IEventGenerator via
        )
        {
            var eventId = Guid.NewGuid();
            var prevEffectType = typeof(BaseEffect);
            var modifiableType = typeof(AttributeModifiableEffect);
            var isModifiable = false;

            return effects.SelectMany(effect =>
            {
                // The same type event generated by an EffectLogic is considered
                // as a whole, use the same event id
                var type = effect.GetType();
                if (type != prevEffectType && (!type.IsSubclassOf(modifiableType) || !isModifiable))
                {
                    prevEffectType = type;
                    eventId = Guid.NewGuid();
                    isModifiable = type.IsSubclassOf(modifiableType);
                }
                
                var events = effect.GenerateEvents(source, via, eventId);
                if (effect.mode != TargetMode.SelectOne)
                    return events;
                
                return events.FindAll(e => e.Target.UniqueId == branchId);
            }).ToList();
        }

        private void HandleChooseActive()
        {
            var players = Context.GetActiveDefeatedPlayers();
            if (players.Count == 0)
                return;
            
            var response = new ChooseActiveResponse(players);

            var player = players.First();
            var branches = player.CharacterLogic.AliveCharacters
                .Select(character => character.UniqueId)
                .ToList();

            Resolving.Analysis();
            Resolving.CreateChildNodes(branches);
            Resolving.Responses.Add(response);

            var childNodes = Resolving.ChildNodes;
            foreach (var (branchId, node) in childNodes)
            {
                Resolving = node;

                var site = player.Id == Context.Id ? Context : Context.Opponent;
                var target = site.CharactersMap.GetValueOrDefault(branchId);
                if (target == null)
                    continue;
                
                var switchEvent = new SwitchActiveEvent(site.ActiveCharacter, target, null);
                var events = switchEvent.SingleList().Cast<BaseEvent>().ToList();

                ExecuteEvents(events);
                
                Resolving.Analysis();
            }
        }
    }
}