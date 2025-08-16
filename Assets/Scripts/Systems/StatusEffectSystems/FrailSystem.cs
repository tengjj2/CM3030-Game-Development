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
    private IEnumerator ApplyFrailPerformer(ApplyFrailGA applyFrailGA)
    {

        CombatantView target = applyFrailGA.Target;
        var caster = applyFrailGA.Caster;
        
        Tween tween = caster.transform.DOMoveX(caster.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        caster.transform.DOMoveX(caster.transform.position.x + 1f, 0.25f);

        int stacksToAdd =  applyFrailGA.BaseAmount;
        target.AddStatusEffect(StatusEffectType.FRAIL, stacksToAdd);
        Instantiate(frailVFX, target.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
    }
}
