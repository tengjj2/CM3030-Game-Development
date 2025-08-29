using UnityEngine;

public class ApplyBarrierGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int Stacks;
    public ApplyBarrierGA(CombatantView target, CombatantView caster, int stacks)
    {
        Target = target;
        Caster = caster;
        Stacks = stacks;
    }
}

