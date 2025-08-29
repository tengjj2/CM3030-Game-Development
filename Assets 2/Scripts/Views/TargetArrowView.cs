using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TargetArrowView : MonoBehaviour
{
    [SerializeField] private GameObject arrowHead;
    [SerializeField] private LineRenderer lineRenderer;
    private Vector3 startPositon;
    private void Update()
    {
        if (Camera.main == null) return;

        Vector3 endPosition = MouseUtil.GetMousePositionInWorldSpace();
        if (endPosition == Vector3.zero) return;

        Vector3 direction = -(startPositon - arrowHead.transform.position).normalized;
        lineRenderer.SetPosition(1, endPosition - direction * 0.5f);
        arrowHead.transform.position = endPosition;
        arrowHead.transform.right = direction;
    }
    public void SetupArrow(Vector3 startPosition)
    {
        this.startPositon = startPosition;
        lineRenderer.SetPosition(0, startPosition);

        // lineRenderer.SetPosition(1, MouseUtil.GetMousePositionInWorldSpace());
        Vector3 mousePosition = MouseUtil.GetMousePositionInWorldSpace();
        if (mousePosition != Vector3.zero) // Added null check
        {
            lineRenderer.SetPosition(1, mousePosition);
        }
    }
}
