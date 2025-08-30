using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckChoiceSystem : Singleton<DeckChoiceSystem>
{
    // Legacy UI (your existing grid/list, keep as fallback)
    [SerializeField] private DeckChoiceUIController ui;

    // New panel UI that uses UICardView via UICardItemButton
    [SerializeField] private DeckChoicePanelUI panel;

    protected override void Awake()
    {
        base.Awake();
        if (!panel) panel = DeckChoicePanelUI.Instance; // in case you forget to wire it
    }

    /// <summary>
    /// Choose N card datas from a pool (multi or single pick decided by 'count').
    /// Calls onPicked with the chosen list (or null on cancel).
    /// </summary>
    public void ChooseCardDatas(List<CardData> pool, int count, Action<List<CardData>> onPicked,
                                string title = "Choose", string prompt = "")
    {
        count = Mathf.Max(1, count);

        // Preferred: new panel (UICardView)
        if (panel != null)
        {
            // multi-select when count > 1; single-pick when count == 1
            bool multi = count > 1;
            panel.ShowChooseFromPool(pool, count, title, prompt, multi, onPicked);
            return;
        }

        // Fallback: legacy UI controller if present
        if (ui != null)
        {
            if (count == 1)
            {
                ui.ShowAdd(title, pool, 1, onPicked); // reuse legacy “add” flow to pick 1
            }
            else
            {
                ui.ShowAdd(title, pool, count, onPicked); // multi-pick if supported
            }
            return;
        }

        Debug.LogWarning("[DeckChoiceSystem] No panel/ui hooked up; returning null choices.");
        onPicked?.Invoke(null);
    }

    /// <summary>
    /// Choose to add 'count' cards from a pool. If panel is present we use it; otherwise legacy UI.
    /// onDone gets the picked list (null if cancelled).
    /// </summary>
    public void ChooseToAdd(List<CardData> pool, int count, Action<List<CardData>> onDone)
    {
        // Use the generic path so both UIs are supported identically
        ChooseCardDatas(pool, count, picked =>
        {
            onDone?.Invoke(picked);
        },
        title: "Add card(s)",
        prompt: (count == 1 ? "Choose a card to add" : $"Choose {count} cards to add"));
    }

    /// <summary>
    /// Choose to remove 'count' cards from the provided deck list.
    /// </summary>
    public void ChooseToRemove(List<CardData> deck, int count, Action<List<CardData>> onDone)
    {
        // Same generic chooser; only prompt text differs
        ChooseCardDatas(deck, count, picked =>
        {
            onDone?.Invoke(picked);
        },
        title: "Remove card(s)",
        prompt: (count == 1 ? "Choose a card to remove" : $"Choose {count} cards to remove"));
    }
}
