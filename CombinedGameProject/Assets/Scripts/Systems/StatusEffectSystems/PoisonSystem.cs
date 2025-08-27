using System.Collections;
using UnityEngine;
using DG.Tweening;
public class PoisonSystem : MonoBehaviour
{
    [SerializeField] private GameObject poisonVFX;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyPoisonGA>(ApplyPoisonPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyPoisonGA>();
    }
    private IEnumerator ApplyPoisonPerformer(ApplyPoisonGA applyPoisonGA)
    {
        var target = applyPoisonGA.Target;
        int stacksToAdd = applyPoisonGA.BaseAmount; // interpret BaseAmount as stacks

        var caster = applyPoisonGA.Caster;
        
        Tween tween = caster.transform.DOMoveX(caster.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        caster.transform.DOMoveX(caster.transform.position.x + 1f, 0.25f);

        int before = target.GetStatusEffectStacks(StatusEffectType.POISON);
        target.AddStatusEffect(StatusEffectType.POISON, stacksToAdd);
        int after  = target.GetStatusEffectStacks(StatusEffectType.POISON);

        if (poisonVFX) Instantiate(poisonVFX, target.transform.position, Quaternion.identity);
        Debug.Log($"[PoisonApply] {target.name} POISON +{stacksToAdd} ({before}→{after}) from {applyPoisonGA.Caster?.name}");

        // Play metal sound effect
        AudioManager.Instance.PlayRandomByPrefix("energy");

        yield return null;

        /*
        CombatantView target = applyPoisonGA.Target;
        Instantiate(poisonVFX, target.transform.position, Quaternion.identity);
        target.Damage(applyPoisonGA.BaseAmount);
        target.RemoveStatusEffect(StatusEffectType.POISON, 1);
        yield return new WaitForSeconds(1f);*/
    }
}
