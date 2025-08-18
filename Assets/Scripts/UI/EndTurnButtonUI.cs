using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnPhaseChanged += HandlePhaseChanged;
            HandlePhaseChanged(TurnSystem.Instance.CurrentPhase); // set initial state
        }
    }

    private void OnDisable()
    {
        if (TurnSystem.Instance != null)
            TurnSystem.Instance.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(TurnSystem.Phase p)
    {
        if (button != null)
            button.interactable = (p == TurnSystem.Phase.Player);
    }

    public void OnClick()
    {
        // Extra guard even if interactable somehow missed an update
        if (TurnSystem.Instance != null && TurnSystem.Instance.CanEndTurn)
            TurnSystem.Instance.EndPlayerTurn();
    }
}