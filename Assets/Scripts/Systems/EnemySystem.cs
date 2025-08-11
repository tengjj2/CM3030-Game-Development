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
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackPlayerGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
    }

    public void Setup(List<EnemyData> enemyDatas)
    {
        for (int i = 0; i < enemyDatas.Count; i++)
        {

            StartCoroutine(enemyBoardView.AddEnemy(enemyDatas[i], i));
        }
    }
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            AttackPlayerGA attackPlayerGA = new(enemy);
            ActionSystem.Instance.AddReaction(attackPlayerGA);
        }
        yield return null;
    }

    private IEnumerator AttackPlayerPerformer(AttackPlayerGA attackPlayerGA)
    {
        EnemyView attacker = attackPlayerGA.Attacker;
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
        DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { PlayerSystem.Instance.PlayerView });
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }

    
    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
    }
}
