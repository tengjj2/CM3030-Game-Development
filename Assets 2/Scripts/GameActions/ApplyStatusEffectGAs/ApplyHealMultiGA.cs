using UnityEngine;
using System.Collections.Generic;

public class ApplyHealMultiGA : GameAction
{
    public List<CombatantView> Targets;
    public CombatantView Caster;
    public int Amount;

    public ApplyHealMultiGA(List<CombatantView> targets, CombatantView caster, int amount)
    {
        Targets = targets != null ? new List<CombatantView>(targets) : new List<CombatantView>();
        Caster  = caster;
        Amount  = amount;
    }
}