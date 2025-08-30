using System.Collections;
using UnityEngine;

public class ThornsSystem : MonoBehaviour
{
    [SerializeField] private GameObject thornsVFX;
    [SerializeField] private bool tweenOnApply = true;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyThornsGA>(ApplyThornsPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyThornsGA>();
    }

    private IEnumerator ApplyThornsPerformer(ApplyThornsGA ga)
    {
        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.Stacks);
        if (target == null || target.Equals(null) || add <= 0) yield break;

        if (tweenOnApply && caster != null && !caster.Equals(null))
        {
            if (SafeCombatant.AbortIfDead(caster, "Thorns(start)")) yield break;
            yield return CombatAnim.StepForwardAndBackIfAlive(caster);
            if (SafeCombatant.AbortIfDead(caster, "Thorns(after tween)")) yield break;
        }

        int before = target.GetStatusEffectStacks(StatusEffectType.THORNS);
        target.AddStatusEffect(StatusEffectType.THORNS, add);
        int after  = target.GetStatusEffectStacks(StatusEffectType.THORNS);

        // Play sound effect
        AudioManager.Instance.PlayRandomByPrefix("energy");

        if (thornsVFX) Instantiate(thornsVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[Thorns] {target.name} +{add} ({before}→{after})");
        yield return null;
    }
}