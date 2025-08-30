using UnityEngine;

public class ApplyEnergyGainGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int Stacks;
    public ApplyEnergyGainGA(CombatantView target, CombatantView caster, int stacks)
    {
        Target = target;
        Caster = caster;
        Stacks = stacks;
    }
}
