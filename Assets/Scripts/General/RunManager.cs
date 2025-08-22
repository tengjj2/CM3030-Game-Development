using System.Collections;
using System;
using UnityEngine;

public class RunManager : Singleton<RunManager>
{
    // NEW: floor lifecycle events
    public static event Action<FloorSO> OnFloorAboutToLoad;  // right before we switch UI/setup
    public static event Action<FloorSO> OnFloorStarted;   
    [SerializeField] private FloorVisibilityController visibility;
    [SerializeField] private BackgroundController background;
    [SerializeField] private RunConfigSO runConfig;   // <- use this instead of FloorSO[]

    private int index = 0;

    public void StartRun()
    {
        index = 0;
        NextFloor();
    }

    public void NextFloor()
    {
        if (runConfig == null || runConfig.Floors == null || index >= runConfig.Floors.Count)
        {
            Debug.Log("Run complete!");
            return;
        }
        TurnSystem.Instance.SuspendCombat();                 // put TurnSystem into Inactive
        DiscardPromptUI.Instance?.Hide();                    // just in case the prompt is up
        CardSystem.Instance.ResetForNextCombat();            // <-- key line
        CostSystem.Instance.Refill(); 
        LoadFloor(runConfig.Floors[index++]);
    }

    public void LoadFloor(FloorSO floor)
    {
        OnFloorAboutToLoad?.Invoke(floor);
        if (floor.Type == FloorType.Lobby)
        {
            visibility?.ShowOnlyLobby();
            BackgroundController.Instance?.SetBackground(floor.BackgroundSprite);

            LobbySystem.Instance.Open(floor.LobbyOffer, () =>
            {
                NextFloor();
            });
            OnFloorStarted?.Invoke(floor);
            return;
        }

        if (floor.Type == FloorType.Combat)
        {
            DiscardPromptUI.Instance?.Hide();
            if (HandView.Instance != null && HandView.Instance.IsSelecting)
            {
                // simple: force it to end by selecting nothing
                // (or add a CancelSelection() API if you prefer)
                // Here we rely on the HandView cleanup in finally when coroutine finishes.
            }

            visibility?.ShowOnlyCombat();
            BackgroundController.Instance?.SetBackground(floor.BackgroundSprite);

            EnemySystem.Instance.Setup(floor.Enemies);
            TurnSystem.Instance.BeginCombat();
            //OnFloorStarted?.Invoke(floor);
            return;
        }
    }
}