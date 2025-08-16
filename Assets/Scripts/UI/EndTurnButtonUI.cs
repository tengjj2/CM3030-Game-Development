using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonUI : MonoBehaviour
{
    [SerializeField] private Button button; // optional, assign in Inspector

    public void OnClick()
    {
        // (Optional) prevent double-clicks while enemy turn runs
        if (button) button.interactable = false;

        // Hand off to TurnSystem; it will Perform(new EnemyTurnGA())
        TurnSystem.Instance.EndPlayerTurn();
    }

    // (Optional) call this from TurnSystem when player turn starts again
    public void EnableButton() { if (button) button.interactable = true; }
}