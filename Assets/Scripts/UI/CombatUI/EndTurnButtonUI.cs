using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;

    private bool _busyQueue   = false; // ActionSystem
    private bool _choosing    = false; // DiscardChoiceSystem
    private bool _dragging    = false; // Interactions (optional)

    private void Awake()
    {
        if (!button) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // Phase
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnPhaseChanged += HandlePhaseChanged;
            HandlePhaseChanged(TurnSystem.Instance.CurrentPhase);
        }

        // Action queue busy
        if (ActionSystem.Instance != null)
            ActionSystem.Instance.OnProcessingChanged += OnProcessingChanged;

        // If you used the fallback gate instead:
        // ActionBusyGate.OnChanged += v => { _busyQueue = v; Refresh(); };

        // Discard choosing
        DiscardChoiceSystem.OnChoosingChanged += OnChoosingChanged;

        // Optional: if you track dragging globally
        _dragging = Interactions.Instance != null && Interactions.Instance.PlayerIsDragging;
        // If Interactions has an event, subscribe; otherwise we'll check in Update
        Refresh();
    }

    private void OnDisable()
    {
        if (TurnSystem.Instance != null)
            TurnSystem.Instance.OnPhaseChanged -= HandlePhaseChanged;

        if (ActionSystem.Instance != null)
            ActionSystem.Instance.OnProcessingChanged -= OnProcessingChanged;

        DiscardChoiceSystem.OnChoosingChanged -= OnChoosingChanged;
    }

    private void Update()
    {
        // If you don't have a dragging event, poll once per frame
        if (Interactions.Instance != null)
        {
            bool drag = Interactions.Instance.PlayerIsDragging;
            if (drag != _dragging) { _dragging = drag; Refresh(); }
        }
    }

    private void OnProcessingChanged(bool busy)
    {
        _busyQueue = busy;
        Refresh();
    }

    private void OnChoosingChanged(bool choosing)
    {
        _choosing = choosing;
        Refresh();
    }

    private void HandlePhaseChanged(TurnSystem.Phase p)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (!button) return;

        bool canClick =
            TurnSystem.Instance != null &&
            TurnSystem.Instance.CanEndTurn &&
            !_busyQueue &&
            !_choosing &&
            !_dragging;

        button.interactable = canClick;
    }

    public void OnClick()
    {
        // Extra hard guard
        if (button != null && !button.interactable) return;

        if (TurnSystem.Instance != null && TurnSystem.Instance.CanEndTurn)
            TurnSystem.Instance.EndPlayerTurn();
    }
}
