using UnityEngine;

public static class StatCalculator
{
    // Returns final damage after all multipliers, rounded DOWN and clamped >= 0
    public static int ComputeFinalDamage(int baseAmount, CombatantView caster, CombatantView target)
    {
        float mult = 1f;

        // ---- Outgoing (on caster) ----
        // Strength: +25% if the caster has any Strength stacks
        if (caster.GetStatusEffectStacks(StatusEffectType.STRENGTH) > 0)
            mult *= 1.25f;

        // Weaken: -25% if the caster has any Weaken stacks
        if (caster.GetStatusEffectStacks(StatusEffectType.WEAKEN) > 0)
            mult *= 0.75f;

        // ---- Incoming (on target) ----
        // Defence: +25% if the target has any Defence stacks
        if (target.GetStatusEffectStacks(StatusEffectType.DEFENCE) > 0)
            mult *= 0.75f;

        // Frail: -25% if the target has any Frail stacks  
        if (target.GetStatusEffectStacks(StatusEffectType.FRAIL) > 0)
            mult *= 1.25f;

        int result = Mathf.FloorToInt(Mathf.Max(0, baseAmount) * mult);
        return Mathf.Max(0, result);
    }
}