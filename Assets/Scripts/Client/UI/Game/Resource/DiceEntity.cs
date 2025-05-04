using System;
using Server.GameLogic;

public class DiceEntity : IComparable<DiceEntity>
{
    public DiceLogic Logic;
    
    public bool Selecting;
    public readonly SmallDice Small;
    public readonly LargeDice Large;
    
    public DiceEntity(SmallDice small, LargeDice large, DiceLogic logic)
    {
        Small = small.SetEntity(this);
        Large = large.SetEntity(this);
        Logic = logic;
    }
    
    public DiceEntity(LargeDice large, DiceLogic logic)
    {
        Large = large.SetEntity(this);
        Logic = logic;
    }

    public void SwitchSelectStatus()
    {
        SetSelectStatus(!Selecting);
    }

    public void SetSelectStatus(bool status)
    {
        Selecting = status;
        Large.SetSelectStatus(status);
        Small?.SetSelectStatus(status);
    }

    public void SetLockingStatus(bool status)
    {
        Large.isLocking = status;
        Large.locking.SetActive(status);
    }

    public int CompareTo(DiceEntity other)
    {
        return other.Logic.Weight.CompareTo(Logic.Weight);
    }
}