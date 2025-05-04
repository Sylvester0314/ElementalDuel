using System;
using System.Collections.Generic;
using System.Linq;
using Client.Logic.Response;
using Server.GameLogic;
using Server.Logic.Event;
using Server.Managers;
using Shared.Classes;
using Shared.Enums;
using Shared.Handler;
using Shared.Misc;
using Events = System.Collections.Generic.List<Server.Logic.Event.BaseEvent>;

namespace Server.ResolveLogic
{
    public class ResolveNode
    {
        public bool IsLeaf;
        public bool Analysed;
        
        public PlayerLogic State;
        public Events Events;
        public List<ulong> FailedPlayers;
        public Dictionary<string, ResolveNode> ChildNodes;

        public ResolveOverview Overview;
        public List<IActionResponse> Responses;
        
        public ActionResponseWrapper[] Wrappers 
            => ActionResponseWrapper.Package(Responses.ToArray());

        public ResolveNode(PlayerLogic initial)
        {
            IsLeaf = true;
            Analysed = false;
            State = GameManager.ClonePlayerState(initial);
            Events = new Events();
            FailedPlayers = new List<ulong>();
            ChildNodes = new Dictionary<string, ResolveNode>();
            Responses = new List<IActionResponse>();

            Overview = new ResolveOverview(initial);
        }

        public void CreateChildNodes(List<string> branches)
        {
            IsLeaf = false;
            ChildNodes = branches.ToDictionary(
                branchId => branchId,
                _ => new ResolveNode(State)
            );
        }

        public void Initialize(ICostHandler via)
        {
            var energy = ResourceLogic
                .GetSpecialCost(via.Cost.Actual, CostType.Energy)
                .FirstOrDefault()?
                .count;
            
            if (energy is not null)
            {
                var active = State.ActiveCharacter.UniqueId;
                var modification = Overview.Modifications[active];

                modification.Modified = true;
                modification.EnergyModified -= energy.Value;
            }
        }
        
        public void Analysis()
        {
            if (Analysed)
                return;

            Analysed = true;
            
            if (!IsLeaf)
            {
                foreach (var node in ChildNodes.Values)
                    node.Analysis();
                return;
            }
            
            var groupedEvents = Events
                .GroupBy(e => e.EventId)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.ToList()
                );
            var analysedEventIds = new HashSet<Guid>();
            
            foreach (var (id, @event) in Events)
            {
                @event.Log();
                if (!analysedEventIds.Add(id))
                    continue;

                var events = groupedEvents[id];
                if (events.TryCast<PreviewableEvent>(out var previewable))
                {
                    foreach (var e in previewable)
                        e.WriteToOverview(Overview);
                }
                
                if (previewable.TryCast<AttributeModifiableEvent>(out var modifiable))
                {
                    var source = modifiable.First().Source;
                    var sourceId = source.UniqueId;
                    var response = new HealthModifiableUnionResponse(sourceId, modifiable);
                        
                    Responses.Add(response);
                    continue;
                }

                foreach (var e in events)
                {
                    Responses.AddRange(e.ToResponses());                
                
                    if (e is not SwitchActiveEvent switchEvent)
                        continue;
                    
                    var target = switchEvent.Target.UniqueId;
                    Overview.Modifications[target].SwitchedTarget = true;
                }
            }

            var unmodified = Overview.Modifications
                .Where(pair => !pair.Value.Modified)
                .Select(pair => pair.Key)
                .ToList();

            foreach (var key in unmodified)
                Overview.Modifications.Remove(key);

            foreach (var pair in Overview.Modifications)
                pair.Value.MergeInitialApplication();
            
            if (FailedPlayers.Count != 0) 
            {
                var response = new GameOverResponse(FailedPlayers);
                Responses.Add(response);
            }
        }

        public (List<DiceLogic> dices, int arcane, int energy) RemoveResource(
            List<CostUnion> costs, List<string> ids
        )
        {
            var dices = State.Resource.Remove(ids, costs, out var arcane, out var energy);

            foreach (var node in ChildNodes.Values)
                node.State.Resource.Remove(ids, costs, out _, out _);
            
            return (dices, arcane, energy);
        }

        public bool CheckGameOver()
        {
            if (FailedPlayers.Count == 0)
                return false;
            
            Analysis();
            
            var response = new GameOverResponse(FailedPlayers);
            Responses.Add(response);
            
            return true;
        }
    }
}