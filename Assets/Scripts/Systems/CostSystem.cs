using System.Collections;
using UnityEngine;

public class CostSystem : Singleton<CostSystem>
{
    [SerializeField] private CostUI costUI;
    private const int MAX_COST = 3;

    // Base energy up to MAX, plus a temporary overflow that can exceed MAX mid-turn
    private int currentcost = MAX_COST;
    private int overflow = 0;

    public int CurrentCost => currentcost + overflow; // display total
    public int MaxCost => MAX_COST;

    protected override void Awake()
    {
        base.Awake(); // keep Singleton<T>’s behaviour
        if (costUI != null)
            costUI.UpdateCostText(CurrentCost);
    }

    void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendCostGA>(SpendCostPerformer);
        ActionSystem.AttachPerformer<RefillCostGA>(RefillCostPerformer);
        ActionSystem.AttachPerformer<ModifyCostGA>(ModifyCostPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendCostGA>();
        ActionSystem.DetachPerformer<RefillCostGA>();
        ActionSystem.DetachPerformer<ModifyCostGA>();
    }

    public bool HasEnoughCost(int cost) => CurrentCost >= cost;

    // ---- Core math (use these everywhere) -----------------

    /// Apply a delta that may overflow above MAX and will also underflow down to 0 safely.
    public void ApplyDeltaAllowOverflow(int delta)
    {
        int total = Mathf.Max(0, CurrentCost + delta);     // never below 0
        currentcost = Mathf.Min(total, MAX_COST);          // fill base up to MAX
        overflow    = total - currentcost;                 // remainder is overflow
        UpdateUI();
        // Debug.Log($"[CostSystem] Δ={delta} → base={currentcost}, overflow={overflow}, total={CurrentCost}");
    }

    /// Set base to MAX and keep no overflow (start-of-turn refill).
    public void Refill()
    {
        currentcost = MAX_COST;
        overflow = 0;
        UpdateUI();
        // Debug.Log($"[CostSystem] Refill → {CurrentCost}");
    }

    /// Clear overflow at end of the player's turn.
    public void ClearOverflow()
    {
        if (overflow != 0)
        {
            overflow = 0;
            UpdateUI();
            // Debug.Log("[CostSystem] ClearOverflow");
        }
    }

    private void UpdateUI()
    {
        if (costUI != null) costUI.UpdateCostText(CurrentCost);
    }

    // ---- Performers --------------------------------------

    private IEnumerator SpendCostPerformer(SpendCostGA ga)
    {
        ApplyDeltaAllowOverflow(-ga.Amount); // spend overflow first, then base
        yield return null;
    }

    private IEnumerator RefillCostPerformer(RefillCostGA _)
    {
        Refill();
        yield return null;
    }

    private IEnumerator ModifyCostPerformer(ModifyCostGA ga)
    {
        // Use overflow-aware path so positive gains can exceed MAX mid-turn
        ApplyDeltaAllowOverflow(ga.Delta);
        yield return null;
    }
}