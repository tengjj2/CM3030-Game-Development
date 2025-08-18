using UnityEngine;


public class ApplyHealGA : GameAction
{
    public CombatantView Target;
    public CombatantView Caster;
    public int Amount;
    public ApplyHealGA(CombatantView target, CombatantView caster, int amount)
    {
        Target  = target;
        Caster  = caster;
        Amount  = amount;
    }
}