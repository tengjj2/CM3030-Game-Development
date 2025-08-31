// ConfuseSystem.cs
using System.Collections;
using UnityEngine;

public class ConfuseSystem : MonoBehaviour
{
    [SerializeField] private GameObject confuseVFX;
    [SerializeField] private bool tweenOnApply = true;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyConfuseGA>(ApplyConfusePerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyConfuseGA>();
    }

    private IEnumerator ApplyConfusePerformer(ApplyConfuseGA ga)
    {
        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.Stacks);
        if (target == null || target.Equals(null) || add <= 0) yield break;

        if (tweenOnApply && caster != null && !caster.Equals(null))
        {
            if (SafeCombatant.AbortIfDead(caster, "Confuse(start)")) yield break;
            yield return CombatAnim.StepForwardAndBackIfAlive(caster);
            if (SafeCombatant.AbortIfDead(caster, "Confuse(after tween)")) yield break;
        }

        int before = target.GetStatusEffectStacks(StatusEffectType.CONFUSE);
        target.AddStatusEffect(StatusEffectType.CONFUSE, add);
        int after  = target.GetStatusEffectStacks(StatusEffectType.CONFUSE);
        if (target is EnemyView ev) ev.RefreshIntentUI();

        // Play sound effect
        AudioManager.Instance.PlayRandomByPrefix("energy");

        if (confuseVFX) Instantiate(confuseVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[Confuse] {target.name} +{add} ({before}→{after})");
        yield return null;
    }
}