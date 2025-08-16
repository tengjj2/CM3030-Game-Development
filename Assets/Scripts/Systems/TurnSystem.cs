using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)] // comes up before typical setup scripts
public class TurnSystem : Singleton<TurnSystem>
{
    private PlayerView PV => PlayerSystem.Instance != null ? PlayerSystem.Instance.PlayerView : null;

    private void OnEnable()
    {
        // Wrap EnemyTurnGA with simple PRE/POST
        ActionSystem.SubscribePerformer<EnemyTurnGA>(OnEnemyTurnPre, ReactionTiming.PRE);
        ActionSystem.SubscribePerformer<EnemyTurnGA>(OnEnemyTurnPost, ReactionTiming.POST);

        // Optionally auto-begin the match from here so you don't touch MatchSetupSystem
        StartCoroutine(AutoBeginWhenReady());
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribePerformer<EnemyTurnGA>(OnEnemyTurnPre, ReactionTiming.PRE);
        ActionSystem.UnsubscribePerformer<EnemyTurnGA>(OnEnemyTurnPost, ReactionTiming.POST);
    }

    /// <summary> If you prefer to call this from MatchSetupSystem, you can; otherwise AutoBeginWhenReady() will call it. </summary>
    public void BeginMatch()
    {
        var pv = PlayerSystem.Instance.PlayerView;
        // Start-of-player-turn TICK, then DRAW
        ActionSystem.Instance.Perform(
            new TickStatusesGA(pv, TickPhase.StartOfTurn, isOwnersTurn: true),
            () => ActionSystem.Instance.Perform(new DrawCardsGA(5))
        );
    }

    // ---------------- Internals ----------------

    private IEnumerator AutoBeginWhenReady()
    {
        // Wait until core systems exist & have done their OnEnable
        // (No edits needed in your other systems)
        while (PlayerSystem.Instance == null || PlayerSystem.Instance.PlayerView == null)
            yield return null;

        while (CardSystem.Instance == null) yield return null;
        yield return null; // extra frame to ensure performers are attached

        // If enemies are spawned via coroutines, optionally wait for them:
        // (Remove if you don't care)
        // while (EnemySystem.Instance == null || EnemySystem.Instance.Enemies.Count == 0) yield return null;

        BeginMatch();
    }

    private void StartPlayerTurn()
    {
        var pv = PlayerSystem.Instance.PlayerView;
        if (pv == null) { Debug.LogWarning("[TurnSystem] No PlayerView"); return; }

        // Poison (and other start-of-turn effects) tick BEFORE drawing
        ActionSystem.Instance.Perform(
            new TickStatusesGA(pv, TickPhase.StartOfTurn, isOwnersTurn: true),
            () => ActionSystem.Instance.Perform(new DrawCardsGA(5))
        );

        Debug.Log("[TurnSystem] StartPlayerTurn → ticked START, then drawing");
    }
    public void EndPlayerTurn()
    {
        // Kick off the enemy phase (PRE will discard; POST will draw)
        ActionSystem.Instance.Perform(new EnemyTurnGA());
    }
    // ---------------- Enemy Turn Envelope ----------------

    private void OnEnemyTurnPre(EnemyTurnGA _)
    {
        var pv = PlayerSystem.Instance.PlayerView;
        if (pv != null)
            ActionSystem.Instance.AddReaction(new TickStatusesGA(pv, TickPhase.EndOfTurn, isOwnersTurn: true));
        // your existing discard, etc.
        ActionSystem.Instance.AddReaction(new DiscardAllCardsGA());
    }

    private void OnEnemyTurnPost(EnemyTurnGA _)
    {
        var pv = PlayerSystem.Instance.PlayerView;
        if (pv != null)
        {
            // Delay 1 second before giving control back to the player
            StartCoroutine(DelayedPlayerTurn(pv));
        }
    }
    
    private IEnumerator DelayedPlayerTurn(PlayerView pv)
    {
        yield return new WaitForSeconds(1f);

        // Queue next player turn start: tick then draw
        ActionSystem.Instance.Perform(
            new TickStatusesGA(pv, TickPhase.StartOfTurn, isOwnersTurn: true),
            () => {
                ActionSystem.Instance.Perform(new RefillCostGA());
                ActionSystem.Instance.Perform(new DrawCardsGA(5));
            }
        );
    }
}