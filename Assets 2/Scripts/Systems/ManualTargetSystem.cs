using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

public class ManualTargetSystem : Singleton<ManualTargetSystem>
{
    [SerializeField] private TargetArrowView targetArrowView;
    [SerializeField] private LayerMask targetLayerMask;
    public void StartTargeting(Vector3 startPosition)
    {
        targetArrowView.gameObject.SetActive(true);
        targetArrowView.SetupArrow(startPosition);
    }
    public EnemyView EndTargeting(Vector3 endPosition)
    {
        targetArrowView.gameObject.SetActive(false);
        if (Physics.Raycast(endPosition, Vector3.forward, out RaycastHit hit, 10f, targetLayerMask)
        && hit.collider != null && hit.transform.TryGetComponent(out EnemyView enemyView))
        {
            return enemyView;
        }
        return null;

    }
}
