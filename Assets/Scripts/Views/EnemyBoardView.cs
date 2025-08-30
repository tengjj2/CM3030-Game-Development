using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyBoardView : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;
    public List<EnemyView> EnemyViews { get; private set; } = new();

    /// <summary>
    /// Spawns an enemy into the slot, animates it in, then adds it to EnemyViews.
    /// Invokes onReady(enemyView) after it is fully placed.
    /// </summary>
    public IEnumerator AddEnemy(EnemyData enemyData, int slotIndex, Action<EnemyView> onReady = null)
    {
        Transform slot = slots[slotIndex];

        // Start a bit off-screen to the right (feel free to tweak)
        Vector3 startPos = slot.position + new Vector3(4f, 0f, 0f);

        // Create the EnemyView (assumes your creator applies sprite/health from enemyData)
        EnemyView enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, startPos, slot.rotation);

        // Parent to slot and move in
        enemyView.transform.SetParent(slot, worldPositionStays: true);
        Tween tween = enemyView.transform.DOMove(slot.position, 0.5f);
        yield return tween.WaitForCompletion();

        // Register to the board
        EnemyViews.Add(enemyView);

        // Callback for wiring extras (AI, intent UI, etc.)
        onReady?.Invoke(enemyView);
    }

    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        EnemyViews.Remove(enemyView);
        Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(enemyView.gameObject);
    }
}