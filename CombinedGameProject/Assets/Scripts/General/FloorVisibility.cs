using UnityEngine;

public class FloorVisibilityController : MonoBehaviour
{
    [Header("Roots (assign the PARENT objects)")]
    [SerializeField] private GameObject combatViewsRoot; // e.g. [World]/CombatViews
    [SerializeField] private GameObject combatUIRoot;    // e.g. Canvas/CombatUI
    [SerializeField] private GameObject lobbyViewsRoot;  // e.g. [World]/LobbyView
    [SerializeField] private GameObject lobbyUIRoot;     // e.g. Canvas/LobbyUI
    [SerializeField] private bool hideDiscardPromptOnShowCombat = true;

    // --- Public API (simple toggles) ---
    public void ShowCombat() => ShowCombat(true);
    public void HideCombat()  => ShowCombat(false);
    public void ShowLobby()   => ShowLobby(true);
    public void HideLobby()   => ShowLobby(false);

    /// <summary>Show/hide all combat elements.</summary>
    public void ShowCombat(bool visible)
    {
        SetActiveSafe(combatViewsRoot, visible);
        SetActiveSafe(combatUIRoot, visible);
        if (visible && hideDiscardPromptOnShowCombat)
            DiscardPromptUI.Instance?.Hide();
    }



    /// <summary>Show/hide all lobby elements.</summary>
    public void ShowLobby(bool visible)
    {
        SetActiveSafe(lobbyViewsRoot, visible);
        SetActiveSafe(lobbyUIRoot, visible);
    }

    /// <summary>Hide lobby, show combat.</summary>
    public void ShowOnlyCombat()
    {
        ShowCombat(true);
        ShowLobby(false);
    }

    /// <summary>Hide combat, show lobby.</summary>
    public void ShowOnlyLobby()
    {
        ShowCombat(false);
        ShowLobby(true);
    }

    // --- Helpers ---
    private static void SetActiveSafe(GameObject go, bool state)
    {
        if (!go) return;
        if (go.activeSelf != state) go.SetActive(state);
    }
    

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Friendly editor warnings if something wasn't assigned
        if (!combatViewsRoot) Debug.LogWarning("[FloorVisibilityController] 'combatViewsRoot' is not assigned.", this);
        if (!combatUIRoot) Debug.LogWarning("[FloorVisibilityController] 'combatUIRoot' is not assigned.", this);
        if (!lobbyViewsRoot) Debug.LogWarning("[FloorVisibilityController] 'lobbyViewsRoot' is not assigned.", this);
        if (!lobbyUIRoot) Debug.LogWarning("[FloorVisibilityController] 'lobbyUIRoot' is not assigned.", this);
    }
#endif
}
