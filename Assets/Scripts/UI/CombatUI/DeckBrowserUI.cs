using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DeckBrowserUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button showDeckButton;
    [SerializeField] private Button showDiscardButton;
    [SerializeField] private Button closeButton;

    [Header("List")]
    [SerializeField] private RectTransform content;   // ScrollRect.content
    [SerializeField] private UICardView cardPrefab;

    [Header("Overlay (optional)")]
    [SerializeField] private GameObject dimBackground; // darken screen when open

    private CanvasGroup cg;
    private readonly List<GameObject> spawned = new();
    private readonly List<CardData>  tempDataToDestroy = new();

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        HideImmediate();

        if (showDeckButton)    showDeckButton.onClick.AddListener(ShowDeck);
        if (showDiscardButton) showDiscardButton.onClick.AddListener(ShowDiscard);
        if (closeButton)       closeButton.onClick.AddListener(Hide);
    }

    // --- Public hooks you can wire from scene buttons ---
    public void ShowDeck()
    {
        var list = CardSystem.Instance?.DrawPileRO;
        OpenWithRuntimeCards(list);
    }

    public void ShowDiscard()
    {
        var list = CardSystem.Instance?.DiscardPileRO;
        OpenWithRuntimeCards(list);
    }

    // If you ever want to show a CardData list directly (e.g., library/rewards)
    public void ShowFromCardDataList(IReadOnlyList<CardData> dataList)
    {
        ClearGrid();

        if (dataList != null && cardPrefab && content)
        {
            foreach (var cd in dataList)
            {
                var view = Instantiate(cardPrefab, content);
                view.SetupFromData(cd);
                spawned.Add(view.gameObject);
            }
        }

        Show();
    }

    // --- Internals ---

    private void OpenWithRuntimeCards(IReadOnlyList<Card> list)
    {
        ClearGrid();

        if (list != null && cardPrefab && content)
        {
            foreach (var c in list)
            {
                if (c == null) continue;

                // Build a temporary CardData from the runtime Card
                var temp = ScriptableObject.CreateInstance<CardData>();
                // Assumes your Card exposes these properties (as used elsewhere in your code)
                temp.name        = c.Name;        // asset name in inspector
                temp.Name        = c.Name;
                temp.Description = c.Description;
                temp.Cost        = c.Cost;
                temp.Image       = c.Image;

                tempDataToDestroy.Add(temp);

                var view = Instantiate(cardPrefab, content);
                view.SetupFromData(temp);
                spawned.Add(view.gameObject);
            }
        }

        Show();
    }

    private void ClearGrid()
    {
        foreach (var go in spawned)
            if (go) Destroy(go);
        spawned.Clear();

        // Clean up temporary ScriptableObjects we created
        foreach (var so in tempDataToDestroy)
            if (so) Destroy(so);
        tempDataToDestroy.Clear();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;
        if (dimBackground) dimBackground.SetActive(true);
    }

    public void Hide()
    {
        cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false;
        if (dimBackground) dimBackground.SetActive(false);
        ClearGrid();
        gameObject.SetActive(false);
    }

    private void HideImmediate()
    {
        cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false;
        if (dimBackground) dimBackground.SetActive(false);
        gameObject.SetActive(false);
    }
}