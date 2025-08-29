using UnityEngine;

public class ApplyConfuseGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int Stacks;
    public ApplyConfuseGA(CombatantView target, CombatantView caster, int stacks)
    {
        Target = target;
        Caster = caster;
        Stacks = stacks;
    }
}
