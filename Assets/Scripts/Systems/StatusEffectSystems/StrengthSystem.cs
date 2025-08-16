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
    private IEnumerator ApplyStrengthPerformer(ApplyStrengthGA applyStrengthGA)
    {
        
        CombatantView target = applyStrengthGA.Target;
        var caster = applyStrengthGA.Caster;
        
        Tween tween = caster.transform.DOMoveX(caster.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        caster.transform.DOMoveX(caster.transform.position.x + 1f, 0.25f);

        int stacksToAdd =  applyStrengthGA.BaseAmount;
        target.AddStatusEffect(StatusEffectType.STRENGTH, stacksToAdd);
        Instantiate(strengthVFX, target.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        /*
        target.RemoveStatusEffect(StatusEffectType.STRENGTH, 1);
        yield return new WaitForSeconds(1f);*/
    }
}
