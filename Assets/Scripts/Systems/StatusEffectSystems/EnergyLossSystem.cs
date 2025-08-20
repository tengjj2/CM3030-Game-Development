using System.Collections;
using UnityEngine;
using DG.Tweening;

public class EnergyLossSystem : MonoBehaviour
{
    [SerializeField] private GameObject energyLossVFX;

    private void OnEnable()
    {
        Debug.Log("[EnergyStatusApplySystem] OnEnable — performers attached");
        ActionSystem.AttachPerformer<ApplyEnergyLossGA>(ApplyLossPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyEnergyLossGA>();
    }

    private IEnumerator ApplyLossPerformer(ApplyEnergyLossGA ga)
    {
        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.Stacks);
        if (target == null || target.Equals(null) || add <= 0) yield break;

        if (caster != null && !caster.Equals(null))
        {
            if (SafeCombatant.AbortIfDead(caster, "EnergyLoss(start)")) yield break;
            yield return CombatAnim.StepForwardAndBackIfAlive(caster);
            if (SafeCombatant.AbortIfDead(caster, "EnergyLoss(after tween)")) yield break;
        }

        int before = target.GetStatusEffectStacks(StatusEffectType.ENERGYLOSS);
        target.AddStatusEffect(StatusEffectType.ENERGYLOSS, add);
        int after  = target.GetStatusEffectStacks(StatusEffectType.ENERGYLOSS);

        if (energyLossVFX) Instantiate(energyLossVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[EnergyLoss] {target.name} stacks +{add} ({before}→{after})");
        yield return null;
    }
}