using UnityEngine;
using UnityEngine.UI;

public class RandomSoundButton : MonoBehaviour
{
    public string soundPrefix = "card";

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayRandomByPrefix(soundPrefix);
        });
    }
}
