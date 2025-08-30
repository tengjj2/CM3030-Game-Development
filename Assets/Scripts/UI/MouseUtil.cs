// using UnityEngine;

// public static class MouseUtil
// {
//     public static Camera camera = Camera.main;

//     public static Vector3 GetMousePositionInWorldSpace(float zValue = 0)
//     {
//         Plane dragPlane = new(camera.transform.forward, new Vector3(0, 0, zValue));
//         Ray ray = camera.ScreenPointToRay(Input.mousePosition);
//         if (dragPlane.Raycast(ray, out float distance))
//         {
//             return ray.GetPoint(distance);
//         }
//         return Vector3.zero;
//     }

// }

using UnityEngine;

public static class MouseUtil
{
    private static Camera _camera;
    public static Camera Camera
    {
        get
        {
            if (_camera == null || !_camera.gameObject.activeInHierarchy)
            {
                _camera = Camera.main;
            }
            return _camera;
        }
    }
    
    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0)
    {
        if (Camera == null) return Vector3.zero;
        
        Plane dragPlane = new(Camera.transform.forward, new Vector3(0, 0, zValue));
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
