// RewardCardButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCardButton : MonoBehaviour
{
    [Header("Wiring (optional)")]
    [SerializeField] private Button button;
    [Header("CardView Preview")]
    [SerializeField] private RectTransform cardHolder; // where the UI card sits
    [SerializeField] private UICardView uiCardPrefab;  // assign in inspector
    [SerializeField] private Vector2 previewSize = new Vector2(420, 600);

    public CardData Data { get; private set; }

    private System.Action<RewardCardButton> onClick;

    private UICardView spawned;

    public void Bind(CardData data, System.Action<RewardCardButton> onClicked)
    {
        Data = data;
        onClick = onClicked;

        if (spawned) Destroy(spawned.gameObject);

        if (cardHolder && uiCardPrefab && Data != null)
        {
            spawned = Instantiate(uiCardPrefab, cardHolder);
            // fix size
            var rt = spawned.GetComponent<RectTransform>();
            if (rt)
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = previewSize;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }
            spawned.SetupFromData(Data);
        }

        if (!button) button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(this));
    }

    public void SetInteractable(bool v)
    {
        if (!button) button = GetComponent<Button>();
        if (button) button.interactable = v;
    }

    private void Awake()
    {
        if (!button) button = GetComponent<Button>();
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(this));
        }
    }

}