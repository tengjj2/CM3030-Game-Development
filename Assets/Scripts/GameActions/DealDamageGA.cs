using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DealDamageGA : GameAction, IHaveCaster
{
    public int BaseAmount { get; set; }
    public int FinalAmount { get; set; }
    public List<CombatantView> Targets { get; set; }
    public CombatantView Caster { get; private set; }
    public DealDamageGA(int amount, List<CombatantView> targets, CombatantView caster)
    {
        BaseAmount = amount;
        FinalAmount = amount;
        Targets = new(targets);
        Caster = caster;

    }
}
