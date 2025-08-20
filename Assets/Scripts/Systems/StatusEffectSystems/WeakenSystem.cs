using UnityEngine;
using System.Collections;
using DG.Tweening;
public class WeakenSystem : MonoBehaviour
{
    [SerializeField] private GameObject weakenVFX;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyWeakenGA>(ApplyWeakenPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyWeakenGA>();
    }
    private IEnumerator ApplyWeakenPerformer(ApplyWeakenGA ga)
{
    var caster = ga.Caster;
    var target = ga.Target;
    int add = Mathf.Max(0, ga.BaseAmount);

    if (SafeCombatant.AbortIfDead(caster, "Weaken(start)")) yield break;
    if (SafeCombatant.AbortIfDead(target, "Weaken(target)")) yield break;

    yield return CombatAnim.StepForwardAndBackIfAlive(caster);

    if (SafeCombatant.AbortIfDead(caster, "Weaken(after tween)")) yield break;
    if (SafeCombatant.AbortIfDead(target, "Weaken(after tween)")) yield break;

    int before = target.GetStatusEffectStacks(StatusEffectType.WEAKEN);
    target.AddStatusEffect(StatusEffectType.WEAKEN, add);
    int after  = target.GetStatusEffectStacks(StatusEffectType.WEAKEN);
    Debug.Log($"[Weaken] {target.name} WEAKEN +{add} ({before}→{after})");
}
}
