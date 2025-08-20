using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BurnSystem : MonoBehaviour
{
    [SerializeField] private GameObject burnVFX;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyBurnGA>(ApplyBurnPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyBurnGA>();
    }

    private IEnumerator ApplyBurnPerformer(ApplyBurnGA ga)
    {
        var target = ga.Target;
        int stacksToAdd = ga.BaseAmount; // interpret BaseAmount as stacks
        var caster = ga.Caster;

        Tween tween = caster.transform.DOMoveX(caster.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        caster.transform.DOMoveX(caster.transform.position.x + 1f, 0.25f);

        int before = target.GetStatusEffectStacks(StatusEffectType.BURN);
        target.AddStatusEffect(StatusEffectType.BURN, stacksToAdd);
        int after  = target.GetStatusEffectStacks(StatusEffectType.BURN);

        if (burnVFX) Instantiate(burnVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[BurnApply] {target.name} BURN +{stacksToAdd} ({before}→{after})");

        yield return null;
    }
}