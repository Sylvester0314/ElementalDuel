using System;

namespace Shared.Enums
{
    [Flags]
    public enum EffectType
    {
        None              = 0,
        Damage            = 1 << 0,
        Heal              = 1 << 1,
        GenerateStatus    = 1 << 2,
        Summon            = 1 << 3,
        Draw              = 1 << 4,
        SwitchActive      = 1 << 5
    }
}