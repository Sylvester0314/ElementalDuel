using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Enums;

namespace Client.Logic.BuildCondition
{
    [Serializable]
    public class PropertyBuildCondition : BaseBuildCondition
    {
        public Property property;
        public int amountLimit = 2;
    
        public override bool CheckCondition(List<CharacterAsset> characters)
        {
            return characters
                       .Where(asset => asset != null)
                       .Count(asset => asset.properties.Contains(property)) 
                   >= amountLimit;
        }
    }
}