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
    private IEnumerator ApplyWeakenPerformer(ApplyWeakenGA applyWeakenGA)
    {

        CombatantView target = applyWeakenGA.Target;
        var caster = applyWeakenGA.Caster;
        
        Tween tween = caster.transform.DOMoveX(caster.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        caster.transform.DOMoveX(caster.transform.position.x + 1f, 0.25f);

        int stacksToAdd =  applyWeakenGA.BaseAmount;
        target.AddStatusEffect(StatusEffectType.WEAKEN, stacksToAdd);
        Instantiate(weakenVFX, target.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
    }
}
