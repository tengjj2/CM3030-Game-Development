using UnityEngine;

public static class MouseUtil
{
    public static Camera camera = Camera.main;
    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0)
    {
        Plane dragPlane = new(camera.transform.forward, new Vector3(0, 0, zValue));
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

}
