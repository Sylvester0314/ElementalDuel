namespace Shared.Enums
{
    public enum SkillType
    {
        NormalAttack,
        ElementalSkill,
        ElementalBurst,
        PassiveSkill,
        SwitchActive,
        Technique
    }

    public enum TargetType
    {
        Active,
        Alive,
        Dead,
        Adjacent,
        Next,
        Previous,
        MostDamaged,
        LeastDamaged,
        HighestHealth,
        LowestHealth,
        Closest,
        Self,
        ExtraZone
    }

    public enum TargetMode
    {
        First,
        AllCharacters = 1,
        SelectOne = 3
    }
}