using UnityEngine;

public class ApplyBlockGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int BaseAmount;
    public ApplyBlockGA(CombatantView target, CombatantView caster, int baseAmount)
    {
        Target = target;
        Caster = caster;
        BaseAmount = baseAmount;
    }
}

