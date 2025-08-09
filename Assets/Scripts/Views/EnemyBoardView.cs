using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyBoardView : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;
    public List<EnemyView> EnemyViews { get; private set; } = new();
    public IEnumerator AddEnemy(EnemyData enemyData, int slotIndex)
    {
        Transform slot = slots[slotIndex];
        Vector3 startPos = slot.position + new Vector3(4f, 0, 0);
        EnemyView enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, startPos, slot.rotation);
        enemyView.transform.parent = slot;
        Tween tween = enemyView.transform.DOMove(slot.position, 0.5f);
        yield return tween.WaitForCompletion();
        EnemyViews.Add(enemyView);
    }

    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        EnemyViews.Remove(enemyView);
        Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(enemyView.gameObject);
    }
}
