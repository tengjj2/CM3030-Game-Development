using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HealSystem : MonoBehaviour
{
    [SerializeField] private GameObject healVFX;
    [SerializeField] private bool tweenOnCast = true;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyHealGA>(ApplyHealPerformer);
        ActionSystem.AttachPerformer<ApplyHealMultiGA>(ApplyHealMultiPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyHealGA>();
        ActionSystem.DetachPerformer<ApplyHealMultiGA>();
    }

    private IEnumerator ApplyHealPerformer(ApplyHealGA ga)
    {
        var caster = ga.Caster;
        var target = ga.Target;
        int heal   = Mathf.Max(0, ga.Amount);

        if (target == null || target.Equals(null) || heal <= 0) yield break;

        // Optional: caster hop anim
        if (tweenOnCast && caster != null && !caster.Equals(null))
        {
            if (SafeCombatant.AbortIfDead(caster, "Heal(start)")) yield break;
            yield return CombatAnim.StepForwardAndBackIfAlive(caster);
            if (SafeCombatant.AbortIfDead(caster, "Heal(after tween)")) yield break;
        }

        // Guard target still alive
        if (SafeCombatant.AbortIfDead(target, "Heal(target)")) yield break;

        // Apply heal (clamped to max)
        int before = target.CurrentHealth;
        target.Heal(heal); // see method below, or inline clamp if you prefer
        int after  = target.CurrentHealth;

        if (healVFX) Instantiate(healVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[Heal] {target.name} +{(after - before)} ({before}→{after})");

        yield return null;
    }

    private IEnumerator ApplyHealMultiPerformer(ApplyHealMultiGA ga)
    {
        var caster = ga.Caster;
        var list   = ga.Targets ?? new List<CombatantView>();
        int heal   = Mathf.Max(0, ga.Amount);

        if (list.Count == 0 || heal <= 0) yield break;

        if (tweenOnCast && caster != null && !caster.Equals(null))
        {
            if (SafeCombatant.AbortIfDead(caster, "HealMulti(start)")) yield break;
            yield return CombatAnim.StepForwardAndBackIfAlive(caster);
            if (SafeCombatant.AbortIfDead(caster, "HealMulti(after tween)")) yield break;
        }

        foreach (var t in list)
        {
            if (t == null || t.Equals(null)) continue;
            if (SafeCombatant.AbortIfDead(t, "HealMulti(target)")) continue;

            int before = t.CurrentHealth;
            t.Heal(heal);
            int after  = t.CurrentHealth;

            if (healVFX) Instantiate(healVFX, t.transform.position, Quaternion.identity);
            Debug.Log($"[Heal] {t.name} +{(after - before)} ({before}→{after})");
            yield return null;
        }
    }
}