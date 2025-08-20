using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class TurnSystem : Singleton<TurnSystem>
{
    public enum Phase { Transition, Player, Enemy }
    public Phase CurrentPhase { get; private set; } = Phase.Transition;
    public bool CanEndTurn => CurrentPhase == Phase.Player;

    public event System.Action<Phase> OnPhaseChanged;

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
        StartCoroutine(AutoBeginWhenReady());
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribePerformer<EnemyTurnGA>(OnEnemyTurnPre,  ReactionTiming.PRE);
        ActionSystem.UnsubscribePerformer<EnemyTurnGA>(OnEnemyTurnPost, ReactionTiming.POST);
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
        StartPlayerTurn(); // sets Player phase
    }

    private void StartPlayerTurn()
    {
        var pv = PlayerSystem.Instance.PlayerView;
        if (pv == null) { Debug.LogWarning("[TurnSystem] No PlayerView"); return; }

        SetPhase(Phase.Player);

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
        if (!CanEndTurn)
        {
            Debug.LogWarning($"[TurnSystem] EndPlayerTurn called while phase={CurrentPhase}");
            return;
        }

        SetPhase(Phase.Transition);
        ActionSystem.Instance.Perform(new EnemyTurnGA()); // PRE/POST hooks wrap enemy phase
    }

    // ---------- Enemy turn envelope ----------

    private void OnEnemyTurnPre(EnemyTurnGA _)
    {
        var pv = PlayerSystem.Instance.PlayerView;

        // Player end-of-turn ticks (ensure Confuse decays here)
        if (pv != null)
            ActionSystem.Instance.AddReaction(new TickStatusesGA(pv, TickPhase.EndOfTurn, isOwnersTurn: true));

        // Usual cleanup
        ActionSystem.Instance.AddReaction(new DiscardAllCardsGA());
    }

    private void OnEnemyTurnPost(EnemyTurnGA _)
    {
        // Add a small pause before giving control back
        StartCoroutine(DelayedPlayerTurn());
    }

    private IEnumerator DelayedPlayerTurn()
    {
        yield return new WaitForSeconds(1f);
        StartPlayerTurn(); // sets phase back to Player
    }
}