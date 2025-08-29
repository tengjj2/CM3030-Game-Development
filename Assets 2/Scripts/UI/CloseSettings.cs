using UnityEngine;
using UnityEngine.SceneManagement;

public class CloseSettings : MonoBehaviour
{
    public void Close()
    {
        UIManager.Instance.CloseSettings();
    }
}
