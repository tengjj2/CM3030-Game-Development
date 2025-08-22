using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DeckChoicePanelUI : Singleton<DeckChoicePanelUI>
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private RectTransform content;          // ScrollRect content
    [SerializeField] private UICardItemButton itemPrefab;    // prefab with UICardView+Button
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Behavior")]
    [SerializeField] private bool allowMultiSelect = false;

    private readonly List<UICardItemButton> spawned = new();
    private readonly List<CardData> selected = new();
    private System.Action<List<CardData>> onDone;
    private int needCount = 1;

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

    // ---- Public API ----
    public void ShowChooseFromPool(
        List<CardData> pool, 
        int countToPick, 
        string title, 
        string prompt,
        bool multiSelect,
        System.Action<List<CardData>> onPicked
    )
    {
        Clear();
        allowMultiSelect = multiSelect;
        needCount = Mathf.Max(1, countToPick);
        onDone = onPicked;

        if (titleText)  titleText.text  = title ?? "";
        if (promptText) promptText.text = prompt ?? "";

        if (pool != null && pool.Count > 0 && itemPrefab && content)
        {
            foreach (var cd in pool)
            {
                var btn = Instantiate(itemPrefab, content);
                btn.Bind(cd, OnItemClicked);
                spawned.Add(btn);
            }
        }

        UpdateConfirmState();
        Show();
    }

    public void Hide()
    {
        HideImmediate();
        Clear();
    }

    // ---- Internals ----
    private void OnItemClicked(UICardItemButton item)
    {
        if (item == null || item.Data == null) return;

        if (!allowMultiSelect)
        {
            // Single pick: select this and auto-confirm
            selected.Clear();
            selected.Add(item.Data);
            foreach (var s in spawned) s.SetSelected(s == item);
            Confirm();
            return;
        }

        // Multi pick: toggle
        if (selected.Contains(item.Data))
        {
            selected.Remove(item.Data);
            item.SetSelected(false);
        }
        else
        {
            if (selected.Count < needCount)
            {
                selected.Add(item.Data);
                item.SetSelected(true);
            }
        }

        UpdateConfirmState();
    }

    private void UpdateConfirmState()
    {
        if (!confirmButton) return;
        if (!allowMultiSelect)
        {
            // hidden in single-pick flow
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

    private void HideImmediate()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void Clear()
    {
        foreach (var go in spawned) if (go) Destroy(go.gameObject);
        spawned.Clear();
        selected.Clear();
        onDone = null;
    }
}
