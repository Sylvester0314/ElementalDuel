using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Logic.Response;
using Server.GameLogic;
using Server.ResolveLogic;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Request
{
    public class UseSkillRequest : BaseRequest, IEquatable<UseSkillRequest>
    {
        public bool IsPreview;
        public string CharacterId;
        public string SkillKey;
        public string Target;
        public List<string> Dices;

        public UseSkillRequest()
        {
            Dices = StaticMisc.EmptyStringList;
        }

        public UseSkillRequest(
            string character, string key, bool preview, List<string> dices,
            string target = ResolveTree.Root
        )
        {
            CharacterId = character;
            SkillKey = key;
            IsPreview = preview;
            Dices = dices;
            Target = target;
        }

        #region Factory Methods

        public static UseSkillRequest Preview(string character, string key)
            => new (character, key, true, StaticMisc.EmptyStringList);

        public static UseSkillRequest Use(string character, string key, List<string> dices, string target)
            => new (character, key, false, dices, target);

        #endregion
        
        public override async Task Process()
        {
            var skill = Logic.FindSkill(CharacterId, SkillKey);
            if (skill == null)
            {
                var promptResponse = PromptResponse.Dialog("no_valid_target");
                TargetResponse(promptResponse);
                return;
            }
            
            if (IsPreview)
                Preview(skill);
            else
                Use(skill);
            
            await base.Process();
        }

        private void Preview(SkillLogic skill)
        {
            var subTree = Game.ResolveManager.CreateSubTree(Logic, skill);
            var overviews = subTree.Overviews;
            
            var response = UseSkillResponse.Preview(RequesterId, CharacterId, SkillKey, overviews);
            
            TargetResponse(response);
            Game.Receiver.Dequeue(UniqueId);
        }

        private void Use(SkillLogic skill)
        {
            var tm = Game.TurnManager;
            if (!tm.Check(RequesterId, UniqueId))
                return;
            
            var valid = Logic.Resource.Check(skill.Cost, Dices);
            var response = UseSkillResponse.Use(RequesterId, CharacterId, SkillKey, valid);
            TargetResponse(response);
            
            if (!valid)
            {
                Game.Receiver.Dequeue(UniqueId);
                return;
            }
            
            tm.DoAction(true);

            var root = Game.ResolveManager.GetSubTreeRoot(skill.Key, Target);
     
            var useCostResponse = ResourceResponse.Use(root, skill, Dices);
            var wrappers = ActionResponseWrapper.Union(useCostResponse, root.Wrappers);
            Response(wrappers);

            Game.ResolveManager.MergeResultToManager(root);
            
            tm.AfterAction(false);
        }
        
        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref IsPreview);
            serializer.SerializeValue(ref CharacterId);
            serializer.SerializeValue(ref SkillKey);
            serializer.SerializeValue(ref Target);
            NetCodeMisc.SerializeList(serializer, ref Dices);
        }

        public bool Equals(UseSkillRequest other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return SkillKey == other.SkillKey &&
                   CharacterId == other.CharacterId &&
                   IsPreview == other.IsPreview;
        }

        public override string ToString()
        {
            return $"UseSkill(Preview={IsPreview}, Character={CharacterId}, Key={SkillKey})";
        }
    }
}