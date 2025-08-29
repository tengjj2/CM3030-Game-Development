using UnityEngine;

public class IncreaseStatsGA : GameAction
{
    //public Minion Target;
    public int AttackIncreaseAmount;
    public int HealthIncreaseAmount;
    public IncreaseStatsGA( int attackIncreaseAmount, int healthIncreaseAmount)
    {
        
        AttackIncreaseAmount = attackIncreaseAmount;
        HealthIncreaseAmount = healthIncreaseAmount;
    }
}
