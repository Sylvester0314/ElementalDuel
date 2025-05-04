using System;

namespace Shared.Enums
{
    [Flags]
    public enum DamageType
    {
        None            = 0,
        NormalAttack    = 1 << 1,
        ElementalSkill  = 1 << 2,
        ElementalBurst  = 1 << 3,
        Status          = 1 << 4,
        Summon          = 1 << 5,
        Swirl           = 1 << 6,
        ChargedAttack   = 1 << 7,
        PlungingAttack  = 1 << 8
    }
}