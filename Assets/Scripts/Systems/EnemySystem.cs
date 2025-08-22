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
                //enemyView.Setup(data);
            }));
        }
    }
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA _)
    {
        var snapshot = new List<EnemyView>(enemyBoardView.EnemyViews);

        foreach (var enemy in snapshot)
        {
            if (enemy == null || enemy.CurrentHealth <= 0) continue;

            // START tick
            ActionSystem.Instance.AddReaction(new TickStatusesGA(enemy, TickPhase.StartOfTurn, isOwnersTurn: true));

            bool wasted = false;
            int confuse = enemy.GetStatusEffectStacks(StatusEffectType.CONFUSE);
            if (confuse > 0 && Random.value < 0.5f)
            {
                wasted = true;
                Debug.Log($"[Confuse] {enemy.name} wastes their turn");
                // little “confused” anim
                yield return enemy.PlayConfusedAnimation();
            }

            if (!wasted)
            {
                var chosen = enemy.AI?.ConsumePlanned(enemy);
                if (chosen?.action != null)
                    chosen.action.Enqueue(enemy);
                else
                    Debug.LogWarning($"[EnemyTurn] {enemy.name} had no action to perform");
            }

            // END tick – always happens (even if wasted)
            ActionSystem.Instance.AddReaction(new TickStatusesGA(enemy, TickPhase.EndOfTurn, isOwnersTurn: true));

            enemy.AI?.TickEndOfTurn();
            if (enemy.CurrentHealth > 0) enemy.PlanNextIntent();
            enemy.UpdateIntentUI(enemy.AI?.NextPlanned);
        }
        yield return null;
    }

    private IEnumerator AttackPlayerPerformer(AttackPlayerGA ga)
    {
        var attacker = ga.Attacker;
        if (SafeCombatant.AbortIfDead(attacker, "Attack(start)")) yield break;

        var player = PlayerSystem.Instance != null ? PlayerSystem.Instance.PlayerView : null;
        if (!SafeCombatant.IsAlive(player)) yield break;

        attacker.PlayAttackAnimation();

        // Cache start position in case attacker moves/dies mid-flow
        var t = attacker.transform;
        float startX = t.position.x;

        // Step forward
        Tween fwd = t.DOMoveX(startX - 1f, 0.15f);
        yield return fwd.WaitForCompletion();

        // Re-check after tween (enemy might have died to a reaction)
        if (SafeCombatant.AbortIfDead(attacker, "Attack(after fwd)")) yield break;
        if (!SafeCombatant.IsAlive(player)) yield break;

        // Step back (don’t block if you prefer overlap; here we wait for cleanliness)
        Tween back = t.DOMoveX(startX, 0.25f);
        yield return back.WaitForCompletion();

        // Final safety before enqueuing damage
        if (SafeCombatant.AbortIfDead(attacker, "Attack(before damage)")) yield break;
        if (!SafeCombatant.IsAlive(player)) yield break;

        // Enqueue damage
        var deal = new DealDamageGA(attacker.AttackPower, new() { player }, ga.Caster);
        ActionSystem.Instance.AddReaction(deal);
    }


    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
        CheckCombatEnd();
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
    
    public void CleanBoard()
    {
        // Remove any previous enemies (if you reuse the same scene)
        var snapshot = new List<EnemyView>(enemyBoardView.EnemyViews);
        foreach (var e in snapshot)
            StartCoroutine(enemyBoardView.RemoveEnemy(e));
    }

    private void CheckCombatEnd()
    {
        bool anyAlive = false;
        foreach (var e in enemyBoardView.EnemyViews)
            if (e != null && e.CurrentHealth > 0) { anyAlive = true; break; }

        if (!anyAlive)
        {
            Debug.Log("[EnemySystem] Combat won!");
            RunManager.Instance.NextFloor();
        }
    }
}
