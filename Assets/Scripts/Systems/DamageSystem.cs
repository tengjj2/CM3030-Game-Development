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

            // ---- THORNS REFLECTION (only on direct attacks) ----
            // If target has thorns, reflect that damage to the attacker
            int thorns = target.GetStatusEffectStacks(StatusEffectType.THORNS);
            if (thorns > 0 && dealDamageGA.Caster != null && !dealDamageGA.Caster.Equals(null))
            {
                var attacker = dealDamageGA.Caster;

                // Optional: tiny feedback on the attacker
                attacker.transform.DOShakePosition(0.15f, 0.25f);

                Debug.Log($"[Thorns] {target.name} thorns={thorns} → {attacker.name} takes {thorns}");
                attacker.Damage(thorns);

                // If thorns kill the attacker, handle it
                if (attacker.CurrentHealth <= 0)
                {
                    if (attacker is EnemyView ae)
                    {
                        ActionSystem.Instance.AddReaction(new KillEnemyGA(ae));
                    }
                    else
                    {
                        // player death logic
                    }
                }
            }
        }
    }
}
