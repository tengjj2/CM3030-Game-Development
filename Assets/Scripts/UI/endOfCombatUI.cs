// CombatEndUI.cs  (with CardLibrarySO integration)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class CombatEndUI : Singleton<CombatEndUI>
{
    [Header("Wiring")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Button nextButton;

    [Header("Card Rewards")]
    [SerializeField] private RectTransform optionsRoot;       // holds a HorizontalLayoutGroup
    [SerializeField] private RewardCardButton optionPrefab;   // prefab with Button + visuals
    [SerializeField] private int optionsCount = 3;
    [SerializeField] private CardLibrarySO cardLibrary;       // <-- assign your CardLibrary asset here

    private readonly List<GameObject> spawned = new();
    private System.Action onNext;
    private bool choiceMade;

    protected override void Awake()
    {
        base.Awake();
        if (!cg) cg = GetComponent<CanvasGroup>();
        HideImmediate();

        if (nextButton)
        {
            nextButton.gameObject.SetActive(false);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => {
                Hide();
                onNext?.Invoke();
            });
        }
        HideCardArea();
    }

    // ------------ Public API ------------
    public void HideImmediate()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void ShowVictory(int gold, int heal, bool pickingCards)
    {
        titleText.text = "Victory!";
        summaryText.text =
            $"Gold: +{gold}\n" +
            (heal > 0 ? $"Healed: +{heal} HP\n" : "") +
            (pickingCards ? "Choose a card:" : "Rewards granted.");

        ShowImmediate();

        if (!pickingCards)
        {
            HideCardArea();
            EnableNext(true, null);
        }
        else
        {
            // If you call ShowVictory with pickingCards=true, call ShowCardChoices next
            EnableNext(false, null);
        }
    }

    /// <summary>
    /// If 'pool' is null/empty, pulls from CardLibrarySO (non-basic, randomized).
    /// </summary>
    public void ShowCardChoices(List<CardData> pool = null, int? count = null, System.Action onDone = null)
    {
        ClearCardArea();
        onNext = onDone;
        choiceMade = false;

        if (!optionsRoot || !optionPrefab)
        {
            // No UI available; just allow next
            EnableNext(true, onDone);
            return;
        }

        // Resolve choices:
        List<CardData> picks = null;

        if (pool != null && pool.Count > 0)
        {
            picks = PickUnique(pool, Mathf.Max(1, count ?? optionsCount));
        }
        else if (cardLibrary != null)
        {
            // Use library to fetch non-basic random rewards
            var need = Mathf.Max(1, count ?? optionsCount);
            picks = cardLibrary.GetRandomRewards(need);
        }
        else
        {
            // Nothing to show
            picks = new List<CardData>();
        }

        if (picks.Count == 0)
        {
            EnableNext(true, onDone);
            return;
        }

        // Spawn buttons
        foreach (var cd in picks)
        {
            var btn = Instantiate(optionPrefab, optionsRoot);
            btn.Bind(cd, OnCardClicked);
            spawned.Add(btn.gameObject);
        }

        optionsRoot.gameObject.SetActive(true);
        // Next stays hidden until a card is chosen
        EnableNext(false, null);
    }

    public void ShowDefeat(System.Action onContinue)
    {
        titleText.text = "Defeat";
        summaryText.text = "Better luck next time.";
        HideCardArea();
        ShowImmediate();
        EnableNext(true, onContinue);
    }

    public void Hide()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        if (nextButton) nextButton.gameObject.SetActive(false);
        HideCardArea();
        gameObject.SetActive(false);
    }

    // ------------ Internals ------------
    private void ShowImmediate()
    {
        gameObject.SetActive(true);
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void EnableNext(bool enabled, System.Action onClick)
    {
        if (!nextButton) return;

        nextButton.onClick.RemoveAllListeners();
        if (enabled && onClick != null)
            nextButton.onClick.AddListener(() => { Hide(); onClick.Invoke(); });

        nextButton.gameObject.SetActive(enabled);
        // Store default for the built-in listener (Awake)
        onNext = onClick ?? onNext;
    }

    private void OnCardClicked(RewardCardButton btn)
    {
        if (!btn || btn.Data == null || choiceMade) return;
        choiceMade = true;

        // Disable all buttons to prevent double picks
        foreach (var go in spawned)
        {
            var r = go ? go.GetComponent<RewardCardButton>() : null;
            if (r) r.SetInteractable(false);
        }

        // Add the chosen card to the player's deck
        CardSystem.Instance?.AddCardToDeck(btn.Data, toTop: false);

        // Reveal Next to finish victory flow
        EnableNext(true, onNext);
    }

    private void HideCardArea()
    {
        if (optionsRoot) optionsRoot.gameObject.SetActive(false);
        ClearCardArea();
    }

    private void ClearCardArea()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
    }

    private static List<CardData> PickUnique(List<CardData> pool, int count)
    {
        var result = new List<CardData>();
        if (pool == null || pool.Count == 0) return result;

        int n = Mathf.Min(count, pool.Count);
        // partial shuffle of indices
        var idx = new List<int>(pool.Count);
        for (int i = 0; i < pool.Count; i++) idx.Add(i);
        for (int i = 0; i < n; i++)
        {
            int r = Random.Range(i, idx.Count);
            (idx[i], idx[r]) = (idx[r], idx[i]);
            result.Add(pool[idx[i]]);
        }
        return result;
    }
}
