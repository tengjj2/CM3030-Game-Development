using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class TurnSystem : Singleton<TurnSystem>
{
    public enum Phase { Transition, Player, Enemy, Inactive }
    public Phase CurrentPhase { get; private set; } = Phase.Inactive;
     public bool CombatActive { get; private set; } = false;
    public bool CanEndTurn => CurrentPhase == Phase.Player;

    public event System.Action<Phase> OnPhaseChanged;

    // Guard to skip any Enemy banner before we’ve shown the very first Player banner
    private bool _firstPlayerBannerShown = false;

    // ---- Called by your floor controller ----
    public void BeginCombat()
    {
        if (CombatActive) return;
        CombatActive = true;
        StartPlayerTurn();
    }

    public void SuspendCombat()
    {
        CombatActive = false;
        SetPhase(Phase.Inactive);
        // (optional) clear hand, etc., if you want: ActionSystem.Instance.Perform(new DiscardAllCardsGA());
    }

    private void SetPhase(Phase p)
    {
        if (CurrentPhase == p) return;
        CurrentPhase = p;
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    private void OnEnable()
    {
        ActionSystem.SubscribePerformer<EnemyTurnGA>(OnEnemyTurnPre,  ReactionTiming.PRE);
        ActionSystem.SubscribePerformer<EnemyTurnGA>(OnEnemyTurnPost, ReactionTiming.POST);
        OnPhaseChanged += HandlePhaseBanner; 
        StartCoroutine(AutoBeginWhenReady());
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribePerformer<EnemyTurnGA>(OnEnemyTurnPre,  ReactionTiming.PRE);
        ActionSystem.UnsubscribePerformer<EnemyTurnGA>(OnEnemyTurnPost, ReactionTiming.POST);
        OnPhaseChanged -= HandlePhaseBanner; // <-- was += before (bug)
    }

    private IEnumerator AutoBeginWhenReady()
    {
        while (PlayerSystem.Instance == null || PlayerSystem.Instance.PlayerView == null) yield return null;
        while (CardSystem.Instance == null) yield return null;
        yield return null;
        BeginMatch();
    }

    public void BeginMatch()
    {
        // Start straight into the player's turn (banner will come from SetPhase)
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        if (!CombatActive) return;

        var pv = PlayerSystem.Instance.PlayerView;
        if (pv == null) { Debug.LogWarning("[TurnSystem] No PlayerView"); return; }

        SetPhase(Phase.Player); // <- Triggers Player banner (once we’re subscribed)

        ActionSystem.Instance.Perform(
            new RefillCostGA(),
            () => ActionSystem.Instance.Perform(
                new TickStatusesGA(pv, TickPhase.StartOfTurn, isOwnersTurn: true),
                () => ActionSystem.Instance.Perform(new DrawCardsGA(5))
            )
        );

        Debug.Log("[TurnSystem] Player turn start → Refill → Tick(START) → Draw");
    }

    public void EndPlayerTurn()
    {
        if (!CombatActive || !CanEndTurn) return;
        if (ActionSystem.Instance != null && ActionSystem.Instance.IsPerforming)
        {
            // If someone called directly, re-queue safely instead of hard-returning.
            ActionSystem.Instance.Perform(new EndTurnRequestGA(), () => EndPlayerTurn());
            return;
        }

        SetPhase(Phase.Transition);
        ActionSystem.Instance.Perform(new EnemyTurnGA());
    }

    // ---------- Enemy turn envelope ----------

    private void OnEnemyTurnPre(EnemyTurnGA _)
    {
        var pv = PlayerSystem.Instance.PlayerView;

        // Flip to Enemy phase (shows “Enemy Turn” banner) — but only after first Player banner is shown
        if (_firstPlayerBannerShown)
            SetPhase(Phase.Enemy);

        // Player end-of-turn ticks & cleanup
        if (pv != null)
            ActionSystem.Instance.AddReaction(new TickStatusesGA(pv, TickPhase.EndOfTurn, isOwnersTurn: true));

        ActionSystem.Instance.AddReaction(new DiscardAllCardsGA());
    }

    private void OnEnemyTurnPost(EnemyTurnGA _)
    {
        // When enemies are done, insert a pacing delay then return to player
        StartCoroutine(DelayedPlayerTurn());
    }

    private IEnumerator DelayedPlayerTurn()
    {
        SetPhase(Phase.Transition);
        yield return new WaitForSeconds(1f);
        StartPlayerTurn(); // sets Phase.Player (banner shows)
    }

    // ---------- Banner driver (ONLY phase changes call UI) ----------

    private void HandlePhaseBanner(Phase p)
    {
        if (TurnBannerUI.Instance == null) return;

        if (p == Phase.Player)
        {
            _firstPlayerBannerShown = true;
            StartCoroutine(TurnBannerUI.Instance.ShowPlayerTurn());
        }
        else if (p == Phase.Enemy)
        {
            if (!_firstPlayerBannerShown) return; // ignore stray enemy during boot
            StartCoroutine(TurnBannerUI.Instance.ShowEnemyTurn());
        }
    }
}