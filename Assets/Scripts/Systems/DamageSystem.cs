using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DamageSystem : Singleton<DamageSystem>
{
    [SerializeField] private GameObject damageVFX;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        // Compute and apply PER-TARGET (targets may have different status)
        foreach (var target in dealDamageGA.Targets)
        {
            int finalDamage = StatCalculator.ComputeFinalDamage(dealDamageGA.BaseAmount, dealDamageGA.Caster, target);

            // If other systems read FinalAmount later, set it for the current target.
            dealDamageGA.FinalAmount = finalDamage;

            target.Damage(finalDamage);

            if (damageVFX != null)
                Instantiate(damageVFX, target.transform.position, Quaternion.identity);

            yield return new WaitForSeconds(0.15f);

            if (target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    ActionSystem.Instance.AddReaction(new KillEnemyGA(enemyView));
                }
                else
                {
                    // player death logic
                }
            }
        }
    }
}
