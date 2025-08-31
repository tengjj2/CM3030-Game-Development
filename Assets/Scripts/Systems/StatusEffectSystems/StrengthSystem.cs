using UnityEngine;
using System.Collections;
using DG.Tweening;

public class StrengthSystem : MonoBehaviour
{
    [SerializeField] private GameObject strengthVFX;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyStrengthGA>(ApplyStrengthPerformer);
        Debug.Log("[StrengthSystem] Attached ApplyStrengthGA performer");
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyStrengthGA>();
    }
    private IEnumerator ApplyStrengthPerformer(ApplyStrengthGA ga)
    {

        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.BaseAmount);

        // Bail if caster/target died or were destroyed before we run
        if (SafeCombatant.AbortIfDead(caster, "Strength(before tween)")) yield break;
        if (SafeCombatant.AbortIfDead(target, "Strength(target check)")) yield break;

        // Optional motion; use the shared helper
        yield return CombatAnim.StepForwardAndBackIfAlive(caster);

        // Re-check after the tween (something else could have killed them)
        if (SafeCombatant.AbortIfDead(caster, "Strength(after tween)")) yield break;
        if (SafeCombatant.AbortIfDead(target, "Strength(after tween)")) yield break;

        int before = target.GetStatusEffectStacks(StatusEffectType.STRENGTH);
        target.AddStatusEffect(StatusEffectType.STRENGTH, add);
        int after  = target.GetStatusEffectStacks(StatusEffectType.STRENGTH);
        if (strengthVFX && SafeCombatant.IsValid(target))
            Instantiate(strengthVFX, target.transform.position, Quaternion.identity);

        // Play metal sound effect
        AudioManager.Instance.PlayRandomByPrefix("metal");

        // Play metal sound effect
        AudioManager.Instance.PlayRandomByPrefix("metal");

        Debug.Log($"[StrengthSystem] {target.name} STRENGTH +{add} ({before}→{after})");
    }
}
