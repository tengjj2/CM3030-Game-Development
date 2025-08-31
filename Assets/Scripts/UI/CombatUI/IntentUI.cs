using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntentUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private GameObject root; // optional: assign a container; falls back to self

    private GameObject Root => root != null ? root : gameObject;

    public void Set(Sprite sprite, int? value)
    {
        if (sprite == null)
        {
            Hide();
            return;
        }

        //ensure it’s visible
        if (!Root.activeSelf) Root.SetActive(true);

        icon.enabled = true;
        icon.sprite = sprite;

        if (value.HasValue)
        {
            valueText.enabled = true;
            valueText.text = value.Value.ToString();
        }
        else
        {
            valueText.enabled = false; // hide number if not provided
            valueText.text = string.Empty;
        }
    }

    public void Hide()
    {
        // hide the widget without destroying it
        if (Root.activeSelf) Root.SetActive(false);
    }
}