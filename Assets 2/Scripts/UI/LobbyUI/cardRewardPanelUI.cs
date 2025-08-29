using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class CardRewardPanelUI : Singleton<CardRewardPanelUI>
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private RectTransform content;          // parent with Horizontal/Vertical Layout
    [SerializeField] private RewardCardButton optionPrefab;  // <-- use RewardCardButton
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Source & Behaviour")]
    [SerializeField] private CardLibrarySO cardLibrary;      // fallback source
    [SerializeField] private int optionsToShow = 3;          // how many buttons/options to display

    private readonly List<GameObject> spawned = new();
    private readonly List<CardData> selected = new();
    private System.Action<List<CardData>> onDone;
    private int needCount = 1;       // how many the user must pick
    private bool multiSelect = false;

    protected override void Awake()
    {
        base.Awake();
        if (!cg) cg = GetComponent<CanvasGroup>();
        HideImmediate();

        if (confirmButton)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(Confirm);
        }
        if (cancelButton)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Cancel);
        }
    }

    /// Show N options; user must pick countToPick (1 = auto‑confirm on click)
    public void ShowChoices(List<CardData> pool, int countToPick, string title, string prompt, System.Action<List<CardData>> onPicked)
    {
        Clear();

        needCount   = Mathf.Max(1, countToPick);
        multiSelect = (needCount > 1);
        onDone      = onPicked;

        if (titleText)  titleText.text  = title ?? "";
        if (promptText) promptText.text = prompt ?? "";

        var src = ResolveSource(pool, Mathf.Max(1, optionsToShow));

        if (src != null && src.Count > 0 && optionPrefab && content)
        {
            foreach (var cd in src)
            {
                if (!cd) continue;
                var btn = Instantiate(optionPrefab, content);
                btn.Bind(cd, OnOptionClicked);  // this spawns the nested UICardView into cardHolder
                spawned.Add(btn.gameObject);
            }
        }

        UpdateConfirmState();
        Show();
    }

    /// Always pull from the library
    public void ShowChoicesFromLibrary(int countToPick, string title, string prompt, System.Action<List<CardData>> onPicked)
    {
        ShowChoices(null, countToPick, title, prompt, onPicked);
    }

    // ---------- Internals ----------
    private void OnOptionClicked(RewardCardButton item)
    {
        if (item == null || item.Data == null) return;

        if (!multiSelect)
        {
            selected.Clear();
            selected.Add(item.Data);

            // disable all to prevent double clicks
            foreach (var go in spawned)
            {
                var r = go ? go.GetComponent<RewardCardButton>() : null;
                if (r) r.SetInteractable(false);
            }

            Confirm();
            return;
        }

        // (If you later support multi‑select with RewardCardButton, add highlight logic here)
        // For now we only use single-pick for rewards.
    }

    private void UpdateConfirmState()
    {
        if (!confirmButton) return;

        // Single-pick flow hides confirm
        if (!multiSelect)
        {
            confirmButton.gameObject.SetActive(false);
            return;
        }

        confirmButton.gameObject.SetActive(true);
        confirmButton.interactable = (selected.Count == needCount);
    }

    private void Confirm()
    {
        var cb = onDone;
        var picks = new List<CardData>(selected);
        Hide();
        cb?.Invoke(picks);
    }

    private void Cancel()
    {
        var cb = onDone;
        Hide();
        cb?.Invoke(null);
    }

    private void Show()
    {
        gameObject.SetActive(true);
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void Hide()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        gameObject.SetActive(false);
        Clear();
    }

    private void HideImmediate()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void Clear()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
        selected.Clear();
        onDone = null;
    }

    private List<CardData> ResolveSource(List<CardData> pool, int displayCount)
    {
        if (pool != null && pool.Count > 0)
            return PickUnique(pool, displayCount);

        if (cardLibrary != null)
        {
            // Your library’s non‑basic reward picker
            var picks = cardLibrary.GetRandomRewards(displayCount);
            return picks ?? new List<CardData>();
        }

        Debug.LogWarning("[CardRewardPanelUI] No pool provided and CardLibrarySO not assigned.");
        return new List<CardData>();
    }

    private static List<CardData> PickUnique(List<CardData> src, int count)
    {
        var result = new List<CardData>();
        if (src == null || src.Count == 0) return result;

        int n = Mathf.Min(count, src.Count);
        var idx = new List<int>(src.Count);
        for (int i = 0; i < src.Count; i++) idx.Add(i);
        for (int i = 0; i < n; i++)
        {
            int r = Random.Range(i, idx.Count);
            (idx[i], idx[r]) = (idx[r], idx[i]);
            var cd = src[idx[i]];
            if (cd) result.Add(cd);
        }
        return result;
    }
}
