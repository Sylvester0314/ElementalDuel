using System;
using System.Collections.Generic;

namespace Client.Logic.BuildCondition
{
    [Serializable]
    public abstract class BaseBuildCondition
    {
        public string conditionDescription;

        public abstract bool CheckCondition(List<CharacterAsset> characters);
    }
}