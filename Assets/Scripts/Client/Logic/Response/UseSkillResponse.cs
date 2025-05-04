using System;
using System.Collections.Generic;
using System.Linq;
using Server.ResolveLogic;
using Shared.Enums;
using Shared.Misc;
using Unity.Netcode;

namespace Client.Logic.Response
{
    public class UseSkillResponse : BaseResponse, IEquatable<UseSkillResponse>
    {
        public bool IsPreview;
        public string CharacterId;
        public string SkillKey;

        public Dictionary<string, ResolveOverview> Overviews;
        public bool Valid;
         
        public UseSkillResponse()
        {
            Overviews = new Dictionary<string, ResolveOverview>();
        }

        public UseSkillResponse(ulong id, string character, string key, bool preview) : base(id)
        {
            Overviews = new Dictionary<string, ResolveOverview>();
            
            CharacterId = character;
            SkillKey = key;
            IsPreview = preview;
        }

        #region Factory Methods

        public static UseSkillResponse Preview(
            ulong id, string character, string key, Dictionary<string, ResolveOverview> overviews)
            => new (id, character, key, true) { Overviews = overviews };

        public static UseSkillResponse Use(ulong id, string character, string key, bool valid)
            => new (id, character, key, false) { Valid = valid };
        
        #endregion

        public override void Process()
        {
            var button = Global.combatAction.GetSkill(CharacterId, SkillKey);

            if (IsPreview)
                OpenPreviewUI(button);
            else if (Valid)
                ClosePreviewUI(button);
            else
            {
                button.DelaySwitchToInitialState();                    
                Global.prompt.dialog.Display(button.SynchronousCost);
            }
            
            base.Process();
        }

        private void OpenPreviewUI(UseSkillButton button)
        {
            Global.Overviews = Overviews;
            Global.hand.ContractAreaLayout(true);
            Global.information.Display(button.Skill);
            Global.prompt.DarkBackgroundDisplay();
            button.chosenEffect.gameObject.SetActive(true);
        
            if (button.Usable)
                Global.diceFunction.OpenChooseDiceUI(button.Matched.dices);
            else
            {
                Global.diceFunction.ResetLayout();
                Global.indicator.Close(false);
                button.DisplayWarningHint();
            }

            var first = Overviews.Keys.First();
            if (button.Skill.Type == SkillType.SwitchActive && Global.switchActiveTarget != null)
                first = Global.switchActiveTarget.uniqueId;

            Global.previewingMainTarget = Global.GetCharacter(first);
            Global.previewingMainTarget.PreviewingAction = button.RequestUse;
            Global.OpenPreviewUI(first);
        }

        private void ClosePreviewUI(UseSkillButton button)
        {
            Global.SetTurnInformationStatus(true);
            Global.information.CloseAll();
            button.isChoosing = false;
            button.ResetGameLayout();
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            
            serializer.SerializeValue(ref IsPreview);
            serializer.SerializeValue(ref CharacterId);
            serializer.SerializeValue(ref SkillKey);
            serializer.SerializeValue(ref Valid);
            
            NetCodeMisc.SerializeDictionary(serializer, ref Overviews);
        }

        public bool Equals(UseSkillResponse other)
        {
            if (ReferenceEquals(other, null) || !base.Equals(other))
                return false;

            return CharacterId.Equals(other.CharacterId) && 
                   SkillKey.Equals(other.SkillKey) &&
                   Valid == other.Valid;
        }
    }
}