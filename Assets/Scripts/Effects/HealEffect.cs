using System.Collections.Generic;
using UnityEngine;
public class HealEffect : Effect
{
    [Min(1)] public int Amount = 6;
    public bool MultiTargetAsGroup = false; // true → one GA for all, false → single target GA (usually fine)

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Default to player if no targets provided
        if (targets == null || targets.Count == 0)
        {
            var pv = PlayerSystem.Instance?.PlayerView;
            if (pv == null) return null;
            return new ApplyHealGA(pv, caster, Amount);
        }

        if (!MultiTargetAsGroup && targets.Count == 1)
            return new ApplyHealGA(targets[0], caster, Amount);

        // Group GA for multiple targets (party-wide heal, etc.)
        return new ApplyHealMultiGA(targets, caster, Amount);
    }
}