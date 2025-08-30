using UnityEngine;
using System.Collections;

public class CurrencySystem : Singleton<CurrencySystem>
{
    [SerializeField] private CurrencyUI ui;
    [SerializeField] private bool persistAcrossScenes = true;

    public int Gold { get; private set; }

    public event System.Action<int> OnGoldChanged;

    protected override void Awake()
    {
        base.Awake();
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
        UpdateUI();
    }

    // --- Public API ---

    /// Set absolute gold (clamped at >= 0)
    public void Set(int amount)
    {
        int clamped = Mathf.Max(0, amount);
        if (clamped == Gold) return;
        Gold = clamped;
        UpdateUI();
    }

    /// Add delta (positive or negative). Negative will be clamped at 0.
    public void Add(int delta, string reason = null)
    {
        if (delta == 0) return;
        int before = Gold;
        Gold = Mathf.Max(0, Gold + delta);
        UpdateUI();
        // Optional debug:
        // Debug.Log($"[Currency] {reason ?? "change"}: {before} -> {Gold} (Δ={delta})");
    }

    /// Try to spend. Returns true if successful.
    public bool Spend(int amount, string reason = null)
    {
        if (amount <= 0) return true;
        if (Gold < amount) return false;
        Gold -= amount;
        UpdateUI();
        // Optional debug:
        // Debug.Log($"[Currency] Spend {amount} for {reason}. Remaining: {Gold}");
        return true;
    }

    public bool TrySpend(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        UpdateUI();
        return true;
    }


    public void AddGold(int amount)
    {
        Gold += Mathf.Max(0, amount);
        OnGoldChanged?.Invoke(Gold);
        UpdateUI();
    }

    public bool Has(int amount) => Gold >= amount;

    private void UpdateUI()
    {
        if (ui) ui.Set(Gold);
        OnGoldChanged?.Invoke(Gold);
    }

    // Optional: performers for GAs (see below)
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddGoldGA>(AddGoldPerformer);
        ActionSystem.AttachPerformer<SpendGoldGA>(SpendGoldPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddGoldGA>();
        ActionSystem.DetachPerformer<SpendGoldGA>();
    }

    private IEnumerator AddGoldPerformer(AddGoldGA ga)
    {
        Add(ga.Amount, ga.Reason);
        yield return null;
    }

    private IEnumerator SpendGoldPerformer(SpendGoldGA ga)
    {
        ga.Success = Spend(ga.Amount, ga.Reason);
        yield return null;
    }
}
