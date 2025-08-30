using UnityEngine;
using UnityEngine.UI;

public class UICardItemButton : MonoBehaviour
{
    [SerializeField] private UICardView uiCardView;
    [SerializeField] private Button button;

    public CardData Data { get; private set; }

    void Reset()
    {
        if (!uiCardView) uiCardView = GetComponentInChildren<UICardView>(true);
        if (!button)     button     = GetComponent<Button>();
    }

    public void Bind(CardData data, System.Action<UICardItemButton> onClick)
    {
        Data = data;
        if (uiCardView) uiCardView.SetupFromData(data);

        if (!button) button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(this));
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        // Optional: tint the frame to show selection
        if (button)
        {
            var colors = button.colors;
            colors.normalColor = selected ? new Color(1f, 1f, 1f, 0.9f) : Color.white;
            button.colors = colors;
        }
    }

    public void SetInteractable(bool v) => button.interactable = v;
}