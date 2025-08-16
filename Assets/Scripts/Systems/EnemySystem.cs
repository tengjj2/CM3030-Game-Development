using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;
    public List<EnemyView> Enemies => enemyBoardView.EnemyViews;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackPlayerGA>(AttackPlayerPerformer);
        //ActionSystem.AttachPerformer<EnemyRemoveDebuffGA>(RemoveDebuffPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackPlayerGA>();
        //ActionSystem.DetachPerformer<EnemyRemoveDebuffGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
    }

    public void Setup(List<EnemyData> enemyDatas)
    {
        for (int i = 0; i < enemyDatas.Count; i++)
        {
            var data = enemyDatas[i];
            StartCoroutine(enemyBoardView.AddEnemy(data, i, (enemyView) =>
            {
                // Let EnemyView.Setup handle AI load/init/intent UI
                enemyView.Setup(data);
            }));
        }
    }
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA _)
    {
        var snapshot = new List<EnemyView>(enemyBoardView.EnemyViews);
        Debug.Log($"[EnemyTurn] Enemies acting: {snapshot.Count}");

        foreach (var enemy in snapshot)
        {
            if (enemy == null || enemy.CurrentHealth <= 0) continue;
            Debug.Log($"[EnemyTurn] {enemy.name} start");

            // Start-of-turn ticks
            ActionSystem.Instance.AddReaction(new TickStatusesGA(enemy, TickPhase.StartOfTurn, true));

            // Do the chosen action
            var chosen = enemy.AI?.ConsumePlanned(enemy);
            if (chosen?.action != null)
            {
                Debug.Log($"[EnemyTurn] {enemy.name} doing {chosen.action.name}");
                chosen.action.Enqueue(enemy);
            }
            else
            {
                Debug.LogWarning($"[EnemyTurn] {enemy.name} had no action to perform");
            }

            // End-of-turn ticks & cooldowns
            ActionSystem.Instance.AddReaction(new TickStatusesGA(enemy, TickPhase.EndOfTurn, true));
            enemy.AI?.TickEndOfTurn();

            // Plan the NEXT intent (and update UI ONCE via EnemyView)
            if (enemy.CurrentHealth > 0)
                enemy.PlanNextIntent();   // <-- This already calls UpdateIntentUI internally
            // REMOVE: enemy.UpdateIntentUI(enemy.AI?.NextPlanned);
        }

        yield return null;
    }

    private IEnumerator AttackPlayerPerformer(AttackPlayerGA attackPlayerGA)
    {
        Debug.Log($"[AttackPlayerGA] performer for {attackPlayerGA.Attacker.name} (source={attackPlayerGA.DebugSource ?? "unknown"})");
        EnemyView attacker = attackPlayerGA.Attacker;
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
        DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { PlayerSystem.Instance.PlayerView }, attackPlayerGA.Caster);
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }


    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
    }

    private IEnumerator RemoveDebuffPerformer(EnemyRemoveDebuffGA enemyRemoveDebuffGA)
    {
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            int frailStacks = enemy.GetStatusEffectStacks(StatusEffectType.FRAIL);
            int weakenStacks = enemy.GetStatusEffectStacks(StatusEffectType.WEAKEN);
            // Weaken decays at the end of THEIR turn
            if (weakenStacks >= 0)
            {
                enemy.RemoveStatusEffect(StatusEffectType.WEAKEN, 1);
            }
            // Weaken decays at the end of THEIR turn
            if (frailStacks >= 0)
            {
                enemy.RemoveStatusEffect(StatusEffectType.FRAIL, 1);
            }
        }
        yield return new WaitForSeconds(1f);
    }
}
