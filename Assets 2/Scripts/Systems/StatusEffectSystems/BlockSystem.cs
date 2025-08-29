using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BlockSystem : MonoBehaviour
{
    [SerializeField] private GameObject blockVFX;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyBlockGA>(ApplyBlockPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyBlockGA>();
    }
    private IEnumerator ApplyBlockPerformer(ApplyBlockGA ga)
    {

        var caster = ga.Caster;
        var target = ga.Target;
        int add = Mathf.Max(0, ga.BaseAmount);
        if (add <= 0) yield break;

        if (SafeCombatant.AbortIfDead(caster, "Block(start)")) yield break;
        if (SafeCombatant.AbortIfDead(target, "Block(target)")) yield break;

        // Small step anim for feedback (uses your helper)
        yield return CombatAnim.StepForwardAndBackIfAlive(caster);

        if (SafeCombatant.AbortIfDead(caster, "Block(after tween)")) yield break;
        if (SafeCombatant.AbortIfDead(target, "Block(after tween)")) yield break;

        int before = target.GetStatusEffectStacks(StatusEffectType.BLOCK);
        target.AddStatusEffect(StatusEffectType.BLOCK, add);
        int after = target.GetStatusEffectStacks(StatusEffectType.BLOCK);

        if (blockVFX) Instantiate(blockVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[BlockSystem] {target.name} BLOCK +{add} ({before}→{after})");

        // Play metal sound effect
        AudioManager.Instance.PlayRandomByPrefix("metal");
    }
}
