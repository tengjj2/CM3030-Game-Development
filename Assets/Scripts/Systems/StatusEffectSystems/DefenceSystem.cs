using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DefenceSystem : MonoBehaviour
{
    [SerializeField] private GameObject defenceVFX;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyDefenceGA>(ApplyDefencePerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyDefenceGA>();
    }
    private IEnumerator ApplyDefencePerformer(ApplyDefenceGA ga)
    {
        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.BaseAmount);

        if (SafeCombatant.AbortIfDead(caster, "Defence(start)")) yield break;
        if (SafeCombatant.AbortIfDead(target, "Defence(target)")) yield break;

        yield return CombatAnim.StepForwardAndBackIfAlive(caster);

        if (SafeCombatant.AbortIfDead(caster, "Defence(after tween)")) yield break;
        if (SafeCombatant.AbortIfDead(target, "Defence(after tween)")) yield break;

        int before = target.GetStatusEffectStacks(StatusEffectType.DEFENCE);
        target.AddStatusEffect(StatusEffectType.DEFENCE, add);
        int after  = target.GetStatusEffectStacks(StatusEffectType.DEFENCE);
        Debug.Log($"[Defence] {target.name} DEFENCE +{add} ({before}→{after})");
    }
}
