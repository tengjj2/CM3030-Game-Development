// BarrierSystem.cs
using System.Collections;
using UnityEngine;

public class BarrierSystem : MonoBehaviour
{
    [SerializeField] private GameObject barrierVFX;
    [SerializeField] private bool tweenOnApply = true;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyBarrierGA>(ApplyBarrierPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyBarrierGA>();
    }

    private IEnumerator ApplyBarrierPerformer(ApplyBarrierGA ga)
    {
        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.Stacks);
        if (target == null || target.Equals(null) || add <= 0) yield break;

        if (tweenOnApply && caster != null && !caster.Equals(null))
        {
            if (SafeCombatant.AbortIfDead(caster, "Barrier(start)")) yield break;
            yield return CombatAnim.StepForwardAndBackIfAlive(caster);
            if (SafeCombatant.AbortIfDead(caster, "Barrier(after tween)")) yield break;
        }

        int before = target.GetStatusEffectStacks(StatusEffectType.BARRIER);
        target.AddStatusEffect(StatusEffectType.BARRIER, add);
        int after  = target.GetStatusEffectStacks(StatusEffectType.BARRIER);

        if (barrierVFX) Instantiate(barrierVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[Barrier] {target.name} +{add} ({before}→{after})");
        yield return null;
    }
}