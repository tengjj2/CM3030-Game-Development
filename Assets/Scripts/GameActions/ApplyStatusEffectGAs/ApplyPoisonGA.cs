using UnityEngine;

public class ApplyPoisonGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int BaseAmount;
    public ApplyPoisonGA(CombatantView target, CombatantView caster, int baseAmount)
    {
        Target = target;
        Caster = caster;
        BaseAmount = baseAmount;
    }
}
