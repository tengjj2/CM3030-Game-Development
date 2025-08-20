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
    private IEnumerator ApplyBlockPerformer(ApplyBlockGA applyBlockGA)
    {
        
        var caster = applyBlockGA.Caster;

        Tween tween = caster.transform.DOMoveX(caster.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        caster.transform.DOMoveX(caster.transform.position.x + 1f, 0.25f);

        CombatantView target = applyBlockGA.Target;
        Instantiate(blockVFX, target.transform.position, Quaternion.identity);
        int blockStacks = target.GetStatusEffectStacks(StatusEffectType.BLOCK);
        //target.RemoveStatusEffect(StatusEffectType.BLOCK, blockStacks);
        yield return new WaitForSeconds(1f);
        
        /*
        int before = applyBlockGA.Target.GetStatusEffectStacks(StatusEffectType.BLOCK);
        applyBlockGA.Target.AddStatusEffect(StatusEffectType.BLOCK, applyBlockGA.BaseAmount);  // or finalAmount
        int after  = applyBlockGA.Target.GetStatusEffectStacks(StatusEffectType.BLOCK);
        Debug.Log($"[BlockSystem] {applyBlockGA.Target.name} BLOCK +{applyBlockGA.BaseAmount} ({before}→{after})");
        yield return null;*/
    }
}
