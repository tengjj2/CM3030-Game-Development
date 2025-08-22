using System.Collections;
using UnityEngine;

public class CostSystem : Singleton<CostSystem>
{
    [Header("UI")]
    [SerializeField] private CostUI costUI;

    [Header("Config")]
    [SerializeField] private int MAX_COST = 3;
    [SerializeField] private bool showDebug = false;

    // Base energy up to MAX, plus temporary overflow (can exceed MAX mid-turn)
    private int currentcost; // base (0..MAX_COST)
    private int overflow;    // >= 0

    public int CurrentCost => currentcost + overflow; // what you can spend now
    public int MaxCost => MAX_COST;

    public System.Action<int> OnCostChanged; // emits CurrentCost after change

    protected override void Awake()
    {
        base.Awake();
        currentcost = MAX_COST; // start full
        overflow = 0;
        UpdateUI();
    }

    private void Log(string msg)
    {
        if (showDebug) Debug.Log($"[CostSystem] {msg}");
    }

    private void UpdateUI()
    {
        if (costUI != null) costUI.UpdateCostText(CurrentCost);
        OnCostChanged?.Invoke(CurrentCost);
    }

    // ---------- Public API (direct) ----------

    /// <summary>
    /// Apply delta with overflow semantics: positive deltas may exceed MAX (into overflow),
    /// negative deltas spend overflow first, then base, never going below 0 total.
    /// </summary>
    public void ApplyDeltaAllowOverflow(int delta)
    {
        if (delta == 0) return;

        if (delta > 0)
        {
            // Fill base up to MAX first, then dump remainder into overflow
            int space = Mathf.Max(0, MAX_COST - currentcost);
            int toBase = Mathf.Min(space, delta);
            currentcost += toBase;
            overflow    += (delta - toBase);
        }
        else // delta < 0
        {
            int spend = -delta;

            // Spend overflow first
            int fromOverflow = Mathf.Min(overflow, spend);
            overflow -= fromOverflow;
            spend    -= fromOverflow;

            // Then spend base
            if (spend > 0)
            {
                currentcost = Mathf.Max(0, currentcost - spend);
                spend = 0;
            }
            // total never below 0 by construction
        }

        Log($"Δ={delta} → base={currentcost}, overflow={overflow}, total={CurrentCost}");
        UpdateUI();
    }

    /// <summary>Start-of-player-turn refill: base → MAX, overflow cleared.</summary>
    public void Refill()
    {
        currentcost = MAX_COST;
        overflow = 0;
        Log($"Refill → {CurrentCost}");
        UpdateUI();
    }

    /// <summary>End-of-turn cleanup: drop any temporary overflow.</summary>
    public void ClearOverflow()
    {
        if (overflow != 0)
        {
            overflow = 0;
            Log("ClearOverflow");
            UpdateUI();
        }
    }

    public bool HasEnoughCost(int cost) => CurrentCost >= cost;

    // ---------- Performers ----------

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendCostGA>(SpendCostPerformer);
        ActionSystem.AttachPerformer<RefillCostGA>(RefillCostPerformer);
        ActionSystem.AttachPerformer<ModifyCostGA>(ModifyCostPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendCostGA>();
        ActionSystem.DetachPerformer<RefillCostGA>();
        ActionSystem.DetachPerformer<ModifyCostGA>();
    }

    private IEnumerator SpendCostPerformer(SpendCostGA ga)
    {
        // Spend using overflow-first rule
        ApplyDeltaAllowOverflow(-ga.Amount);
        yield return null;
    }

    private IEnumerator RefillCostPerformer(RefillCostGA _)
    {
        Refill();
        yield return null;
    }

    private IEnumerator ModifyCostPerformer(ModifyCostGA ga)
    {
        // Positive: immediate mid-turn gains can exceed cap (overflow)
        // Negative: immediate drains (e.g., Energy Loss)
        ApplyDeltaAllowOverflow(ga.Delta);
        yield return null;
    }
}