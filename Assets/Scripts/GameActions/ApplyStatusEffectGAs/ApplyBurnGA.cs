using UnityEngine;

public class ApplyBurnGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int BaseAmount;
    public ApplyBurnGA(CombatantView target, CombatantView caster, int baseAmount)
    {
        Target = target;
        Caster = caster;
        BaseAmount = baseAmount;
    }
}
