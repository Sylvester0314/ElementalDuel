using System;
using Server.GameLogic;

namespace Shared.Logic.CharacterFilter
{
    [Serializable]
    public abstract class BaseFilter
    {
        public abstract bool Check(CharacterData character);
    }
}