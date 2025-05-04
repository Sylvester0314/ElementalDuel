using System;
using System.Collections.Generic;

namespace Client.Logic.BuildCondition
{
    [Serializable]
    public class EmptyBuildCondition : BaseBuildCondition
    {
        public override bool CheckCondition(List<CharacterAsset> _)
        {
            return true;
        }
    }
}