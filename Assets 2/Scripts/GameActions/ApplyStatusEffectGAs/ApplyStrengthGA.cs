using UnityEngine;

public class ApplyStrengthGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int BaseAmount;
    public ApplyStrengthGA(CombatantView target, CombatantView caster, int baseAmount)
    {
        Target = target;
        Caster = caster;
        BaseAmount = baseAmount;
    }
}

