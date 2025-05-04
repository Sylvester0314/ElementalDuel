using System;
using Server.GameLogic;

namespace Shared.Logic.CharacterFilter
{
    [Serializable]
    public class StatusAttachedFilter : BaseFilter
    {
        public StatusCardAsset status;
        public bool negative;
        
        public override bool Check(CharacterData character)
            => negative ^ character.Statuses.Contains(status);
    }
}