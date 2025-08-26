using UnityEngine;
using UnityEngine.UI;

public class PlayRandomSound : MonoBehaviour
{
    public string soundPrefix = "card";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.Instance.PlayRandomByPrefix(soundPrefix);
        }
    }
}
