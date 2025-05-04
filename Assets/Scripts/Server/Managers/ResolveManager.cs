using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Logic.Response;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Handler;
using Shared.Logic.Effect;
using UnityEngine;

namespace Server.Managers
{
    public class ResolveManager
    {
        public GameManager Game;
        public Dictionary<string, ResolveTree> SubTrees;

        public ResolveManager(GameManager game)
        {
            Game = game;
            SubTrees = new Dictionary<string, ResolveTree>();
        }

        public void CleanCache()
        {
            var keys = SubTrees.Keys.ToList();
            SubTrees.Clear();
            var sb = new StringBuilder();
            sb.AppendLine($"Cleared current resolve managers count({keys.Count}): ");
            keys.ForEach(k => sb.AppendLine(k));
            Debug.Log(sb.ToString());
        }

        public ResolveTree CreateSubTree(PlayerLogic player, IEventGenerator entity)
        {
            if (SubTrees.TryGetValue(entity.Key, out var existedResolve))
                return existedResolve;
            
            var subTree = new ResolveTree(Game, player);
            
            if (entity is SkillLogic skill)
                subTree.UseSkill(skill);
            if (entity is ActionCard card)
                subTree.PlayCard(card);
            
            SubTrees.Add(entity.Key, subTree);
            return subTree;
        }

        public ActionResponseWrapper[] PassiveTrigger(Timing timing, PlayerLogic player = null)
        {
            player ??= Game.TurnManager.FirstActive;

            var node = new ResolveTree(Game, player).Trigger(timing);

            return MergeResultToManager(node);
        }

        public ActionResponseWrapper[] PassiveTrigger(BaseCreateEffect effect, PlayerLogic player = null)
        {
            player ??= Game.TurnManager.FirstActive;

            var node = new ResolveTree(Game, player).Trigger(effect);
            
            return MergeResultToManager(node);
        }

        public ActionResponseWrapper[] MergeResultToManager(ResolveNode node)
        {
            var state = node.State;
            var triggerId = state.Id;
            Game.BranchesTemp = node.ChildNodes;

            foreach (var (playerId, i) in Game.MappingId)
                Game.Logics[i] = playerId == triggerId 
                    ? state : state.Opponent;

            return node.Wrappers;
        }

        public ResolveNode GetSubTreeRoot(string key, string target) 
            => SubTrees[key].Branches[target];
    }
}