using UnityEngine;

public class ApplyEnergyLossGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int Stacks;
    public ApplyEnergyLossGA(CombatantView target, CombatantView caster, int stacks)
    {
        Target = target;
        Caster = caster;
        Stacks = stacks;
    }
}
