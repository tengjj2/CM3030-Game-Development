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
    private IEnumerator ApplyDefencePerformer(ApplyDefenceGA applyDefenceGA)
    {
        CombatantView target = applyDefenceGA.Target;
        var caster = applyDefenceGA.Caster;
        
        Tween tween = caster.transform.DOMoveX(caster.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        caster.transform.DOMoveX(caster.transform.position.x + 1f, 0.25f);

        int stacksToAdd =  applyDefenceGA.BaseAmount;
        target.AddStatusEffect(StatusEffectType.DEFENCE, stacksToAdd);
        Instantiate(defenceVFX, target.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
    }
}
