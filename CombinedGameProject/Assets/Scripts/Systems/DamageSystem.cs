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

            // play attack sound
            AudioManager.Instance.PlayRandomByPrefix("punch");

            if (damageVFX != null)
                Instantiate(damageVFX, target.transform.position, Quaternion.identity);

            yield return new WaitForSeconds(0.15f);

            if (target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    ActionSystem.Instance.AddReaction(new KillEnemyGA(enemyView));
                    // After resolving an enemy death, check if all are dead → Victory
                    if (EnemySystem.Instance != null)
                    {
                        bool anyAlive = false;
                        var list = EnemySystem.Instance.Enemies; // assume you expose a List<EnemyView> Enemies
                        if (list != null)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                var e = list[i];
                                if (e != null && e.CurrentHealth > 0) { anyAlive = true; break; }
                            }
                        }
                        if (!anyAlive)
                        {
                            var floor = RunManager.Instance?.CurrentFloor;
                            var cardPool = CombatEndUI.Instance.CardLibrary.GetRandomRewards(3); // 3 options
                            int pickCount = 1; // let player pick 1 card

                            int goldReward = floor != null ? floor.GoldReward : 0;
                            ActionSystem.Instance.AddReaction(new CombatVictoryGA(
                            gold: goldReward,
                            healAmount: 0,
                            cardRewardPool: cardPool,
                            pickCardCount: pickCount));
                        }
                    }
                }
                else
                {
                    
                    // player death logic
                    ActionSystem.Instance.AddReaction(new CombatDefeatGA());
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
