using System.Collections.Generic;
using UnityEngine;
public class EnergyGainEffect : Effect
{
    [Min(1)] public int Amount = 1;

    // Your EffectSystem calls this with (targets, caster)
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // pick a sensible default if no target list supplied (player gains energy)
        CombatantView target = null;

        if (targets != null && targets.Count > 0)
            target = targets[0];
        else
            target = PlayerSystem.Instance?.PlayerView; // default to player

        if (target == null) return null;

        // This GA will (via EnergyGainSystem) show the icon AND immediately add energy
        return new ApplyEnergyGainGA(target, caster, Amount);
    }
}