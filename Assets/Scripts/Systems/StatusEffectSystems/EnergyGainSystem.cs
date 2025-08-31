using System.Collections;
using UnityEngine;

public class EnergyGainSystem : MonoBehaviour
{
    [SerializeField] private GameObject energyGainVFX;
    [SerializeField] private bool tweenOnApply = true;

    private void OnEnable()
    {
        Debug.Log("[EnergyGainSystem] OnEnable — performer attached");
        ActionSystem.AttachPerformer<ApplyEnergyGainGA>(ApplyGainPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyEnergyGainGA>();
    }

    private IEnumerator ApplyGainPerformer(ApplyEnergyGainGA ga)
    {
        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.Stacks);
        if (target == null || target.Equals(null) || add <= 0) yield break;

        // Optional hop on caster (guarded)
        if (tweenOnApply && caster != null && !caster.Equals(null))
        {
            if (SafeCombatant.AbortIfDead(caster, "EnergyGain(start)")) yield break;
            yield return CombatAnim.StepForwardAndBackIfAlive(caster);
            if (SafeCombatant.AbortIfDead(caster, "EnergyGain(after tween)")) yield break;
        }

        // Show status icon immediately (visual for this turn)
        int before = target.GetStatusEffectStacks(StatusEffectType.ENERGYGAIN);
        target.AddStatusEffect(StatusEffectType.ENERGYGAIN, add);
        int after  = target.GetStatusEffectStacks(StatusEffectType.ENERGYGAIN);
        if (energyGainVFX) Instantiate(energyGainVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[EnergyGain] {target.name} stacks +{add} ({before}→{after})");

        // Play energy sound effect
        AudioManager.Instance.PlayRandomByPrefix("energy");

        // 🔑 Instant energy: usable right now
        ActionSystem.Instance.AddReaction(new ModifyCostGA(+add));

        yield return null;
    }
}