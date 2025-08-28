using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    public string soundName = "click_button";

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.Instance.Play(soundName);
        });
    }
}
