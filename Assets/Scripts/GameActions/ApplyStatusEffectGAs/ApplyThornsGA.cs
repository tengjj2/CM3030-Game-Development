using UnityEngine;

public class ApplyThornsGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int Stacks;
    public ApplyThornsGA(CombatantView target, CombatantView caster, int stacks)
    {
        Target = target;
        Caster = caster;
        Stacks = stacks;
    }
}
