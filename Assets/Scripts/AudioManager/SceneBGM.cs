using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    public string bgmName;

    void Start()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(bgmName))
        {
            AudioManager.Instance.PlayBGM(bgmName);
        }
    }
}
