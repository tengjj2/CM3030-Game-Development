using UnityEngine;
using System.Collections;
using DG.Tweening;
public class FrailSystem : MonoBehaviour
{
    [SerializeField] private GameObject frailVFX;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyFrailGA>(ApplyFrailPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyFrailGA>();
    }
    private IEnumerator ApplyFrailPerformer(ApplyFrailGA ga)
    {
        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.BaseAmount);

        if (SafeCombatant.AbortIfDead(caster, "Frail(start)")) yield break;
        if (SafeCombatant.AbortIfDead(target, "Frail(target)")) yield break;

        yield return CombatAnim.StepForwardAndBackIfAlive(caster);

        if (SafeCombatant.AbortIfDead(caster, "Frail(after tween)")) yield break;
        if (SafeCombatant.AbortIfDead(target, "Frail(after tween)")) yield break;

        // Play sound effect
        AudioManager.Instance.PlayRandomByPrefix("energy");

        int before = target.GetStatusEffectStacks(StatusEffectType.FRAIL);
        target.AddStatusEffect(StatusEffectType.FRAIL, add);
        int after  = target.GetStatusEffectStacks(StatusEffectType.FRAIL);
        Debug.Log($"[Frail] {target.name} FRAIL +{add} ({before}→{after})");
    }
}
