using UnityEngine;

public class ApplyDefenceGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int BaseAmount;
    public ApplyDefenceGA(CombatantView target, CombatantView caster, int baseAmount)
    {
        Target = target;
        Caster = caster;
        BaseAmount = baseAmount;
    }
}
