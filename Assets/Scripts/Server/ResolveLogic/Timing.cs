namespace Server.ResolveLogic
{
    public enum Timing
    {
        // Phase
        OnBattleBegin                   = 0,
        OnRollPhase,
        OnEndPhase,
        OnDeclareRoundEnd,
        OnRoundEnd,
        OnActionPhaseBeginning,
        BeforeActionChosen,
        AfterSkillUsed,
        
        // Resource
        OnDiceIncrease                  = 100,
        OnDiffDiceDecrease,
        OnTargetDiceDecrease,
        OnAnyDiceDecrease,
        
        // Damage
        OnDamageDealing                 = 200,
        AfterDamageDealt,
        OnDamageTaking,
        AfterDamageTaken,
        OnElementalInfuse,
        OnDamageIncrease,
        OnDamageAmplify,
        OnDamageReduce,
        OnDamageDecrease,
        OnImmuneToDefeat,
        
        // Heal
        OnHealReceiving              = 300,
        AfterHealReceived,
        
        // Switch Active Character
        OnCharacterSwitchPerforming     = 400,
        OnCharacterSwitching,
        AfterCharacterSwitched,
        
        // Misc
        
        // Modify some attributes of the action
        // e.g. Elemental Infuse, Fast Action
        OnModifyActionAttribute         = 900
    }
}