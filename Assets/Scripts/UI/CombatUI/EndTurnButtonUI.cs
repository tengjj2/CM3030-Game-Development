using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonUI : Singleton<EndTurnButtonUI>
{
    [SerializeField] private Button button;

    private bool pendingEndTurn = false;

    protected override void Awake()
    {
        base.Awake();
        if (!button) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnPhaseChanged += HandlePhaseChanged;
            Refresh();
        }
    }

    private void OnDisable()
    {
        if (TurnSystem.Instance != null)
            TurnSystem.Instance.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(TurnSystem.Phase p)
    {
        // clear debounce when we actually leave the player phase
        if (p != TurnSystem.Phase.Player) pendingEndTurn = false;
        Refresh();
    }

    private void Refresh()
    {
        bool canEnd = TurnSystem.Instance != null && TurnSystem.Instance.CanEndTurn;
        bool queueBusy = ActionSystem.Instance != null && ActionSystem.Instance.IsPerforming;
        if (button) button.interactable = canEnd && !pendingEndTurn && !queueBusy;
    }

    public void OnClick()
    {
        // Guard: only during player phase
        if (TurnSystem.Instance == null || !TurnSystem.Instance.CanEndTurn) return;
        if (pendingEndTurn) return; // debounce
        pendingEndTurn = true;
        Refresh();

        // Schedule end-turn to occur AFTER the current action queue drains.
        // No-op GA ensures this runs in-order with any still-queued reactions from the card play.
        ActionSystem.Instance.Perform(new EndTurnRequestGA(), () =>
        {
            // When we get here, the queue is idle for this frame; do the actual phase swap.
            TurnSystem.Instance.EndPlayerTurn();
            // Button will re-enable when phase changes back to Player.
        });
    }
}
